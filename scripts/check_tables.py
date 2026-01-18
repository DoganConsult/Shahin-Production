#!/usr/bin/env python3
"""
Check database tables
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

def check_tables():
    """Check what tables exist."""
    conn = None
    try:
        print(f"Connecting to database...")
        conn = psycopg2.connect(**DB_CONFIG)
        cur = conn.cursor()

        # Check for user-related tables
        cur.execute("""
            SELECT table_name
            FROM information_schema.tables
            WHERE table_schema = 'public'
            AND (table_name ILIKE '%user%' OR table_name ILIKE '%aspnet%')
            ORDER BY table_name
        """)
        print("\nUser-related tables:")
        for row in cur.fetchall():
            print(f"  - {row[0]}")

        # Check foreign key on TenantUsers
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
            WHERE tc.table_name = 'TenantUsers'
            AND tc.constraint_type = 'FOREIGN KEY'
        """)
        print("\nForeign keys on TenantUsers:")
        for row in cur.fetchall():
            print(f"  - {row[0]}: {row[1]}.{row[2]} -> {row[3]}.{row[4]}")

        # Check if there's an ApplicationUser table
        cur.execute("""
            SELECT column_name, data_type
            FROM information_schema.columns
            WHERE table_name = 'ApplicationUser'
            ORDER BY ordinal_position
            LIMIT 5
        """)
        app_user_cols = cur.fetchall()
        if app_user_cols:
            print("\nApplicationUser table columns (first 5):")
            for row in app_user_cols:
                print(f"  - {row[0]} ({row[1]})")
        else:
            print("\nApplicationUser table does not exist")

        # Check AspNetUsers table
        cur.execute("""
            SELECT column_name, data_type
            FROM information_schema.columns
            WHERE table_name = 'AspNetUsers'
            ORDER BY ordinal_position
            LIMIT 5
        """)
        aspnet_cols = cur.fetchall()
        if aspnet_cols:
            print("\nAspNetUsers table columns (first 5):")
            for row in aspnet_cols:
                print(f"  - {row[0]} ({row[1]})")
        else:
            print("\nAspNetUsers table does not exist")

    except Exception as e:
        print(f"ERROR: {e}")
    finally:
        if conn:
            conn.close()

if __name__ == '__main__':
    check_tables()
