#!/usr/bin/env python3
"""
Check Workspaces table columns
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

def check_columns():
    """Check Workspaces columns."""
    conn = None
    try:
        print(f"Connecting to database...")
        conn = psycopg2.connect(**DB_CONFIG)
        cur = conn.cursor()

        # Get Workspaces columns
        cur.execute("""
            SELECT column_name, data_type, is_nullable, column_default
            FROM information_schema.columns
            WHERE table_name = 'Workspaces'
            ORDER BY ordinal_position
        """)
        print("\nWorkspaces columns:")
        for row in cur.fetchall():
            nullable = "NULL" if row[2] == 'YES' else "NOT NULL"
            default = f" DEFAULT {row[3]}" if row[3] else ""
            print(f"  {row[0]:30} {row[1]:20} {nullable}{default}")

    except Exception as e:
        print(f"ERROR: {e}")
    finally:
        if conn:
            conn.close()

if __name__ == '__main__':
    check_columns()
