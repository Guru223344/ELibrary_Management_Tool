/* =====================================================================
   E-Library Management  |  SQL Server 2016+  |  Database: ElibraryDB
   Run this whole script once in SSMS (or: sqlcmd -S .\SQLEXPRESS -i schema.sql)
   ===================================================================== */
IF DB_ID('ElibraryDB') IS NULL CREATE DATABASE ElibraryDB;
GO
USE ElibraryDB;
GO

/* ---------- Tables ---------- */
CREATE TABLE admin_login_tbl (
    username       NVARCHAR(50)  NOT NULL CONSTRAINT pk_admin PRIMARY KEY,
    password_hash  NVARCHAR(200) NOT NULL,
    full_name      NVARCHAR(100) NOT NULL
);

CREATE TABLE author_master_tbl (
    author_id    NVARCHAR(20)  NOT NULL CONSTRAINT pk_author PRIMARY KEY,
    author_name  NVARCHAR(100) NOT NULL
);

CREATE TABLE publisher_master_tbl (
    publisher_Id    NVARCHAR(20)  NOT NULL CONSTRAINT pk_publisher PRIMARY KEY,
    publisher_name  NVARCHAR(100) NOT NULL
);

CREATE TABLE member_master_tbl (
    member_id       NVARCHAR(30)  NOT NULL CONSTRAINT pk_member PRIMARY KEY,
    full_name       NVARCHAR(100) NOT NULL,
    dob             DATE          NOT NULL,
    contact_no      VARCHAR(15)   NOT NULL,
    email           NVARCHAR(150) NOT NULL CONSTRAINT uq_member_email UNIQUE,
    state           NVARCHAR(60)  NOT NULL,
    city            NVARCHAR(60)  NOT NULL,
    pincode         VARCHAR(10)   NOT NULL,
    full_address    NVARCHAR(300) NOT NULL,
    password_hash   NVARCHAR(200) NOT NULL,
    account_status  VARCHAR(10)   NOT NULL CONSTRAINT df_member_status DEFAULT 'Pending'
                    CONSTRAINT ck_member_status CHECK (account_status IN ('Pending','Active','Deactive')),
    created_at      DATETIME2     NOT NULL CONSTRAINT df_member_created DEFAULT SYSUTCDATETIME()
);

CREATE TABLE book_master_tbl (
    book_id           NVARCHAR(30)  NOT NULL CONSTRAINT pk_book PRIMARY KEY,
    book_name         NVARCHAR(200) NOT NULL,
    genre             NVARCHAR(300) NULL,          -- comma separated, e.g. "Science, Textbook"
    author_id         NVARCHAR(20)  NOT NULL CONSTRAINT fk_book_author    FOREIGN KEY REFERENCES author_master_tbl(author_id),
    publisher_id      NVARCHAR(20)  NOT NULL CONSTRAINT fk_book_publisher FOREIGN KEY REFERENCES publisher_master_tbl(publisher_Id),
    publish_date      DATE          NULL,
    book_language     NVARCHAR(30)  NOT NULL,
    edition           NVARCHAR(30)  NULL,
    book_cost         DECIMAL(10,2) NOT NULL CONSTRAINT ck_book_cost  CHECK (book_cost >= 0),
    no_of_pages       INT           NULL         CONSTRAINT ck_book_pages CHECK (no_of_pages > 0),
    book_description  NVARCHAR(MAX) NULL,
    actual_stock      INT           NOT NULL     CONSTRAINT ck_actual_stock CHECK (actual_stock >= 0),
    current_stock     INT           NOT NULL,
    book_img_link     NVARCHAR(255) NULL,
    CONSTRAINT ck_stock_range CHECK (current_stock >= 0 AND current_stock <= actual_stock)
);
CREATE INDEX ix_book_author    ON book_master_tbl(author_id);
CREATE INDEX ix_book_publisher ON book_master_tbl(publisher_id);

CREATE TABLE book_issue_tbl (
    issue_id     INT IDENTITY(1,1) NOT NULL CONSTRAINT pk_issue PRIMARY KEY,
    member_id    NVARCHAR(30)  NOT NULL CONSTRAINT fk_issue_member FOREIGN KEY REFERENCES member_master_tbl(member_id),
    book_id      NVARCHAR(30)  NOT NULL CONSTRAINT fk_issue_book   FOREIGN KEY REFERENCES book_master_tbl(book_id),
    issue_date   DATE          NOT NULL,
    due_date     DATE          NOT NULL,
    return_date  DATE          NULL,
    fine_amount  DECIMAL(10,2) NOT NULL CONSTRAINT df_issue_fine DEFAULT 0,
    CONSTRAINT ck_issue_dates CHECK (due_date >= issue_date)
);
CREATE INDEX ix_issue_member ON book_issue_tbl(member_id, return_date);
CREATE INDEX ix_issue_book   ON book_issue_tbl(book_id);
-- a member cannot hold two copies of the same book at once
CREATE UNIQUE INDEX ux_active_issue ON book_issue_tbl(member_id, book_id) WHERE return_date IS NULL;

CREATE TABLE ai_query_log_tbl (          -- audit + per-user rate limiting for the AI Librarian
    id          INT IDENTITY(1,1) NOT NULL CONSTRAINT pk_ai_log PRIMARY KEY,
    member_id   NVARCHAR(30)  NULL,
    question    NVARCHAR(500) NOT NULL,
    created_at  DATETIME2     NOT NULL CONSTRAINT df_ai_created DEFAULT SYSUTCDATETIME()
);
CREATE INDEX ix_ai_log_member ON ai_query_log_tbl(member_id, created_at);
GO

/* ---------- Views ---------- */
CREATE VIEW vw_book_catalog AS
SELECT b.book_id, b.book_name, b.genre, b.book_language, b.edition, b.publish_date, b.book_cost, b.no_of_pages,
       b.book_description, b.book_img_link, b.actual_stock, b.current_stock, b.current_stock AS available,
       a.author_id, a.author_name, p.publisher_Id, p.publisher_name
FROM book_master_tbl b
JOIN author_master_tbl    a ON a.author_id    = b.author_id
JOIN publisher_master_tbl p ON p.publisher_Id = b.publisher_id;
GO

CREATE VIEW vw_issued_books AS
SELECT i.issue_id, i.member_id, m.full_name AS member_name, i.book_id, b.book_name,
       i.issue_date, i.due_date, i.return_date, i.fine_amount,
       CASE WHEN i.return_date IS NOT NULL THEN 'Returned'
            WHEN i.due_date < CAST(GETDATE() AS DATE) THEN 'Overdue'
            ELSE 'Issued' END AS status
FROM book_issue_tbl i
JOIN member_master_tbl m ON m.member_id = i.member_id
JOIN book_master_tbl   b ON b.book_id   = i.book_id;
GO

/* ---------- Stored procedures (business rules live next to the data) ---------- */
CREATE PROCEDURE sp_IssueBook
    @member_id NVARCHAR(30), @book_id NVARCHAR(30), @issue_date DATE, @due_date DATE
AS
BEGIN
    SET NOCOUNT ON; SET XACT_ABORT ON;
    IF @due_date < @issue_date THROW 50001, 'Due date cannot be before the issue date.', 1;
    IF DATEDIFF(DAY, @issue_date, @due_date) > 30 THROW 50002, 'Maximum loan period is 30 days.', 1;
    BEGIN TRAN;
    IF NOT EXISTS (SELECT 1 FROM member_master_tbl WHERE member_id = @member_id AND account_status = 'Active')
        THROW 50003, 'Member not found or account is not active.', 1;
    IF (SELECT COUNT(*) FROM book_issue_tbl WHERE member_id = @member_id AND return_date IS NULL) >= 5
        THROW 50005, 'Member already has 5 books issued.', 1;
    UPDATE book_master_tbl SET current_stock = current_stock - 1 WHERE book_id = @book_id AND current_stock > 0;
    IF @@ROWCOUNT = 0 THROW 50004, 'Book not found or out of stock.', 1;
    INSERT INTO book_issue_tbl(member_id, book_id, issue_date, due_date) VALUES (@member_id, @book_id, @issue_date, @due_date);
    COMMIT;
END
GO

CREATE PROCEDURE sp_ReturnBook
    @member_id NVARCHAR(30), @book_id NVARCHAR(30), @return_date DATE
AS
BEGIN
    SET NOCOUNT ON; SET XACT_ABORT ON;
    BEGIN TRAN;
    DECLARE @issue_id INT, @due DATE;
    SELECT @issue_id = issue_id, @due = due_date
    FROM book_issue_tbl WITH (UPDLOCK)
    WHERE member_id = @member_id AND book_id = @book_id AND return_date IS NULL;
    IF @issue_id IS NULL THROW 50006, 'No active issue found for this member and book.', 1;
    DECLARE @fine DECIMAL(10,2) = CASE WHEN @return_date > @due THEN DATEDIFF(DAY, @due, @return_date) * 5.00 ELSE 0 END;  -- Rs 5 per late day
    UPDATE book_issue_tbl SET return_date = @return_date, fine_amount = @fine WHERE issue_id = @issue_id;
    UPDATE book_master_tbl SET current_stock = current_stock + 1 WHERE book_id = @book_id;
    COMMIT;
    SELECT @fine AS fine_amount;
END
GO

/* ---------- Seed data ---------- */
-- admin / Admin@123   and   member demo / Demo@1234   (CHANGE THESE after first login)
INSERT INTO admin_login_tbl VALUES ('admin', 'v1$100000$XxyafjstTGChjg9H0rnGoQ==$XtiTqoMWf3UEFo/UIrypFJT7DzIx1xLMFy0SdesbUmA=', 'Library Admin');
INSERT INTO member_master_tbl(member_id, full_name, dob, contact_no, email, state, city, pincode, full_address, password_hash, account_status)
VALUES ('demo', 'Demo Member', '2000-01-01', '9999999999', 'demo@example.com', 'Telangana', 'Hyderabad', '500001', 'Sample address',
        'v1$100000$o9GcDneySF+ebRDEuD9Sqg==$KPu+2Fu38mdXFks/RVHfW0qi53y9R69Z8+vb03OkPZ4=', 'Active');

INSERT INTO author_master_tbl VALUES ('A001','Robert C. Martin'), ('A002','Andrew Hunt'), ('A003','Yuval Noah Harari'),
                                     ('A004','James Clear'), ('A005','Frank Herbert'), ('A006','J. R. R. Tolkien');
INSERT INTO publisher_master_tbl VALUES ('P001','Prentice Hall'), ('P002','Addison-Wesley'), ('P003','Harper'),
                                        ('P004','Avery'), ('P005','Chilton Books'), ('P006','Allen & Unwin');
INSERT INTO book_master_tbl(book_id, book_name, genre, author_id, publisher_id, publish_date, book_language, edition, book_cost, no_of_pages, book_description, actual_stock, current_stock) VALUES
('B001','Clean Code','Textbook, Science','A001','P001','2008-08-01','English','1st',450,464,'Practical principles for writing readable, maintainable software.',5,5),
('B002','The Pragmatic Programmer','Textbook, Science','A002','P002','1999-10-20','English','1st',500,352,'Tips and habits for becoming a more effective software developer.',4,4),
('B003','Sapiens','History, Science','A003','P003','2011-01-01','English','1st',399,443,'A sweeping history of humankind from the Stone Age to today.',6,6),
('B004','Atomic Habits','Self Help, Motivation','A004','P004','2018-10-16','English','1st',350,320,'How tiny daily changes compound into remarkable results.',8,8),
('B005','Dune','Science Fiction, Adventure','A005','P005','1965-08-01','English','1st',299,412,'Politics, religion and ecology collide on the desert planet Arrakis.',3,3),
('B006','The Hobbit','Fantasy, Adventure','A006','P006','1937-09-21','English','1st',250,310,'A reluctant hobbit joins a quest to reclaim a dragon-guarded treasure.',5,5);
GO
