# Database design – `ElibraryDB` (SQL Server)

```mermaid
erDiagram
    author_master_tbl    ||--o{ book_master_tbl : writes
    publisher_master_tbl ||--o{ book_master_tbl : publishes
    member_master_tbl    ||--o{ book_issue_tbl  : borrows
    book_master_tbl      ||--o{ book_issue_tbl  : "is issued in"
    author_master_tbl    { nvarchar author_id PK
                           nvarchar author_name }
    publisher_master_tbl { nvarchar publisher_Id PK
                           nvarchar publisher_name }
    member_master_tbl    { nvarchar member_id PK
                           nvarchar full_name
                           date dob
                           varchar contact_no
                           nvarchar email UK
                           nvarchar state
                           nvarchar city
                           varchar pincode
                           nvarchar full_address
                           nvarchar password_hash
                           varchar account_status "Pending|Active|Deactive" }
    book_master_tbl      { nvarchar book_id PK
                           nvarchar book_name
                           nvarchar genre "comma separated"
                           nvarchar author_id FK
                           nvarchar publisher_id FK
                           date publish_date
                           nvarchar book_language
                           decimal book_cost
                           int actual_stock
                           int current_stock "0..actual_stock"
                           nvarchar book_img_link }
    book_issue_tbl       { int issue_id PK
                           nvarchar member_id FK
                           nvarchar book_id FK
                           date issue_date
                           date due_date
                           date return_date "NULL = still out"
                           decimal fine_amount }
    admin_login_tbl      { nvarchar username PK
                           nvarchar password_hash
                           nvarchar full_name }
    ai_query_log_tbl     { int id PK
                           nvarchar member_id
                           nvarchar question
                           datetime2 created_at }
```

| Object | Purpose |
|---|---|
| `vw_book_catalog` | Book + author + publisher in one row; used by the grids and the AI Librarian |
| `vw_issued_books` | Issue history with a computed status (Issued / Overdue / Returned) |
| `sp_IssueBook` | Transactional issue: member must be Active, stock > 0, max 5 open loans, max 30 days |
| `sp_ReturnBook` | Transactional return: restores stock, fine = Rs 5 per late day |
| `ux_active_issue` | Unique filtered index: one open loan per member per book |

**Connection:** the app reads the single connection string named `con` in `Web.config`. Change `Data Source` to your instance
(`.\SQLEXPRESS`, `(localdb)\MSSQLLocalDB`, or `localhost`). Only parameterized queries / stored procedures touch the database (`Infrastructure/Db.cs`).
