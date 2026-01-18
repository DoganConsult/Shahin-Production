# Cloudflare Route Configuration Guide

**Date**: January 15, 2026  
**Current Step**: Configuring hostname route for `shahin-ai.com`

---

## ✅ What You've Configured

- **Hostname**: `shahin-ai.com`
- **Tunnel**: `Production-Shahin-ai`
- **Description**: `website hostname`

---

## ⚠️ Important: You Need to Configure the Service

The hostname route tells Cloudflare **which hostname** to use, but you also need to tell it **where to send traffic** (the service/application).

---

## 🎯 Two Possible Approaches

### **Approach 1: Use "Public Hostnames" Tab (Recommended)**

This is usually easier - it combines hostname + service in one form:

1. **Go back to the tunnel overview**
   - Click "Back" or navigate to: **Networks** → **Connectors** → **Your Tunnel**

2. **Look for "Public Hostnames" tab**
   - This tab typically has a form that includes:
     - Subdomain/Domain
     - Service Type (HTTP/HTTPS)
     - Service URL (`http://localhost:5000`)

3. **Add Public Hostname**:
   - **Subdomain**: (leave blank for root) or `portal`
   - **Domain**: `shahin-ai.com`
   - **Service Type**: `HTTP`
   - **Service URL**: `http://localhost:5000`
   - **Path**: (leave blank for all paths)
   - Click: **Save**

### **Approach 2: Complete Current Route + Add Application**

If you're using the "Hostname Routes" approach:

1. **Save the current hostname route** (click "Save")

2. **Find the "Applications" or "Services" section**
   - Look for tabs: "Applications", "Services", or "Published applications"
   - Or look for a "+ Add application" button

3. **Add Application/Service**:
   - **Name**: `shahin-ai-app` (or any name)
   - **Type**: `HTTP`
   - **URL**: `http://localhost:5000`
   - **Associate with route**: Select `shahin-ai.com` route you just created
   - Click: **Save**

---

## 📋 What to Look For

### In "Public Hostnames" Tab:
- Form fields: Subdomain, Domain, Service Type, Service URL
- This is the **easiest** option

### In "Applications" Tab:
- Application name, Type, URL fields
- Option to link to hostname route

### In "Hostname Routes" Tab:
- Just hostname + tunnel (what you're currently configuring)
- Need to add service separately

---

## ✅ Recommended Configuration

**For each subdomain, create:**

1. **Main Domain** (`shahin-ai.com`):
   - Subdomain: (blank)
   - Domain: `shahin-ai.com`
   - Service: `http://localhost:5000`

2. **Portal** (`portal.shahin-ai.com`):
   - Subdomain: `portal`
   - Domain: `shahin-ai.com`
   - Service: `http://localhost:5000`

3. **WWW** (`www.shahin-ai.com`):
   - Subdomain: `www`
   - Domain: `shahin-ai.com`
   - Service: `http://localhost:5000`

---

## 🔍 If You Can't Find Service Configuration

**Try these locations:**

1. **After saving the route**, look for:
   - "Add application" button
   - "Configure service" link
   - "Edit" button on the route (might have service settings)

2. **Check different tabs**:
   - "Public Hostnames" (most common)
   - "Applications"
   - "Services"
   - "Published applications"

3. **Look for a "+" or "Add" button** near the route you just created

---

## ⚠️ Important Notes

- **Service URL must be**: `http://localhost:5000` (your application)
- **Tunnel must be running**: ✅ Already running
- **Application must be running**: ✅ Already running on port 5000
- **DNS must be configured**: ✅ Already done (CNAME records)

---

## 🚀 After Configuration

1. **Save all routes**
2. **Wait 1-2 minutes** for propagation
3. **Test**: Visit `https://shahin-ai.com`
4. **Expected**: Should see your application

---

**Need Help?** If you can't find where to add the service URL, try the "Public Hostnames" tab - it's usually the easiest way to configure everything in one place.
