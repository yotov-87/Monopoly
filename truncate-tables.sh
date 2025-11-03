#!/bin/bash

# Script to truncate all tables in the monopoly database
# This preserves the schema but removes all data

PGPASSWORD=yourpassword psql -h localhost -U yourusername -d monopoly -f truncate-tables.sql

echo ""
echo "✅ All tables truncated successfully!"
