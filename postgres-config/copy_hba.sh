#!/bin/bash
echo "🔁 Copying custom pg_hba.conf into data directory..."
cp /tmp/pg_hba.conf /var/lib/postgresql/data/pg_hba.conf