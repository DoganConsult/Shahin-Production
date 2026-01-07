#!/bin/bash
# Migration script to update services from GrcDbContext to IDbContextFactory<GrcDbContext>
# This script helps identify and update services systematically

echo "=== Service Migration Helper ==="
echo ""
echo "Services that need migration:"
echo ""

# Find all services with direct GrcDbContext injection
grep -r "GrcDbContext _context\|private readonly GrcDbContext\|public.*\(GrcDbContext" src/GrcMvc/Services --include="*.cs" | \
    cut -d: -f1 | sort -u | while read file; do
    echo "  - $file"
done

echo ""
echo "=== Migration Pattern ==="
echo ""
echo "BEFORE:"
echo "  private readonly GrcDbContext _context;"
echo "  public MyService(GrcDbContext context) { _context = context; }"
echo "  await _context.Evidences.ToListAsync();"
echo ""
echo "AFTER:"
echo "  private readonly IDbContextFactory<GrcDbContext> _contextFactory;"
echo "  public MyService(IDbContextFactory<GrcDbContext> contextFactory) { _contextFactory = contextFactory; }"
echo "  await using var context = _contextFactory.CreateDbContext();"
echo "  await context.Evidences.ToListAsync();"
echo ""
echo "=== TenantId Filter Audit ==="
echo ""
echo "Found TenantId filters (keep for safety even with database-per-tenant):"
grep -r "\.Where.*TenantId\|TenantId.*==\|TenantId.*!=" src/GrcMvc --include="*.cs" | wc -l | xargs echo "Total:"
echo ""
