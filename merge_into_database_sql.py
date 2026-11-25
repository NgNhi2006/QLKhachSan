with open('/sessions/dazzling-gracious-gates/mnt/QLKhachSan/Database/ChuanHoa_7_Bang.sql', 'r', encoding='utf-8') as f:
    migration_sql = f.read()

# Let's save ChuanHoa_7_Bang.sql as a dedicated, easy-to-run file and also update Database.sql
with open('/sessions/dazzling-gracious-gates/mnt/QLKhachSan/Database/Database.sql', 'w', encoding='utf-8') as f:
    f.write(migration_sql)

print("Updated Database/Database.sql to include complete migration and cleanup logic!")
