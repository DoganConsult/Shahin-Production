#!/usr/bin/env python3
"""
Check relationship between AspNetUsers and ApplicationUser tables
"""
import psycopg2

# Database connection settings
DB_CONFIG = {
    'host': 'caboose.proxy.rlwy.net',
    'port': 11527,
    'database': 'GrcMvcDb',
    'user': 'postgres',
    'password': 'QNcTvViWopMfCunsyIkkXwuDpufzhkLs',
    'sslmode': 'require'
}

def check_relationship():
    """Check relationship between AspNetUsers and ApplicationUser."""
    conn = None
    try:
        print(f"Connecting to database...")
        conn = psycopg2.connect(**DB_CONFIG)
        cur = conn.cursor()

        # Check table types
        cur.execute("""
            SELECT table_name, table_type
            FROM information_schema.tables
            WHERE table_name IN ('AspNetUsers', 'ApplicationUser')
        """)
        print("\nTable types:")
        for row in cur.fetchall():
            print(f"  {row[0]}: {row[1]}")

        # Check foreign keys on AspNetUserRoles
        cur.execute("""
            SELECT
                tc.constraint_name,
                tc.table_name,
                kcu.column_name,
                ccu.table_name AS foreign_table_name,
                ccu.column_name AS foreign_column_name
            FROM information_schema.table_constraints AS tc
            JOIN information_schema.key_column_usage AS kcu
                ON tc.constraint_name = kcu.constraint_name
            JOIN information_schema.constraint_column_usage AS ccu
                ON ccu.constraint_name = tc.constraint_name
            WHERE tc.table_name = 'AspNetUserRoles'
            AND tc.constraint_type = 'FOREIGN KEY'
        """)
        print("\nForeign keys on AspNetUserRoles:")
        for row in cur.fetchall():
            print(f"  {row[0]}: {row[1]}.{row[2]} -> {row[3]}.{row[4]}")

        # Check if there's any data in AspNetUsers
        cur.execute('SELECT COUNT(*) FROM "AspNetUsers"')
        aspnet_count = cur.fetchone()[0]
        cur.execute('SELECT COUNT(*) FROM "ApplicationUser"')
        appuser_count = cur.fetchone()[0]
        print(f"\nRecord counts:")
        print(f"  AspNetUsers: {aspnet_count}")
        print(f"  ApplicationUser: {appuser_count}")

        # Check if same IDs exist in both
        if aspnet_count > 0:
            cur.execute('SELECT "Id", "Email" FROM "AspNetUsers" LIMIT 3')
            print("\nSample AspNetUsers:")
            for row in cur.fetchall():
                print(f"  ID: {row[0][:20]}..., Email: {row[1]}")

        if appuser_count > 0:
            cur.execute('SELECT "Id", "Email" FROM "ApplicationUser" LIMIT 3')
            print("\nSample ApplicationUser:")
            for row in cur.fetchall():
                email = row[1] if row[1] else "NULL"
                print(f"  ID: {row[0][:20]}..., Email: {email}")

    except Exception as e:
        print(f"ERROR: {e}")
    finally:
        if conn:
            conn.close()

if __name__ == '__main__':
    check_relationship()
