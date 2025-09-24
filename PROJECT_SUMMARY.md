# TM3 CFS Simple C# Project Summary

## 📋 Project Overview

This is a **C# console application** that demonstrates how to download Municipal Market Monitor (TM3) bulk files using the LSEG Refinitiv Data Platform (RDP) Client File Store (CFS) API.

| **Attribute** | **Details** |
|---------------|-------------|
| **Project Name** | TM3_CFS_SimpleCSharp |
| **Version** | 1.0.0 |
| **Last Update** | August 2025 |
| **Framework** | .NET 8.0 |
| **Language** | C# |
| **Project Type** | Console Application |

---

## 🎯 Purpose

The application serves as a **step-by-step demonstration** of how to:
- Authenticate with RDP APIs
- Query for TM3 file sets using the CFS API
- Download bulk financial data files
- Handle token management (refresh and revocation)

> **Note**: This is demonstration code intended for learning purposes and is not optimized for production use.

---

## 🏗️ Project Structure

```
TM3_CFS_SimpleCSharp/
├── LICENSE.md                    # Apache 2.0 License
├── README.md                     # Detailed setup and usage instructions
└── TM3Console/                   # Main console application
    ├── Program.cs                # Main application logic (533 lines)
    ├── Token.cs                  # Token model class
    ├── TM3Console.csproj         # Project configuration
    ├── TM3Console.sln            # Solution file
    ├── bin/                      # Build output directory
    └── obj/                      # Build artifacts
```

---

## 🔧 Technical Details

### **Dependencies**
- **Newtonsoft.Json** (13.0.3) - JSON serialization/deserialization
- **DotNetEnv** (3.1.1) - Environment variable management

### **Key Components**

#### **Program.cs** - Main Application Logic
- **Authentication**: OAuth2 login with RDP
- **File Querying**: Search for TM3 file sets with pagination support
- **File Download**: Download bulk files from cloud storage
- **Token Management**: Refresh and revoke tokens

#### **Token.cs** - Authentication Model
- Manages RDP access tokens
- Handles token expiration and refresh mechanisms

---

## 🚀 Key Features

### **1. Authentication Workflow**
- OAuth2 authentication with RDP APIs
- Automatic token refresh before expiration
- Secure token revocation on session end

### **2. File Management**
- Query file sets by modification date (`modifiedSince` parameter)
- Pagination support for large result sets (100 records per page)
- Smart filtering to limit data retrieval

### **3. Bulk File Download**
- Direct download from cloud storage URLs
- Support for large TM3 data files
- Built-in download progress tracking

### **4. Configuration Management**
- Environment-based configuration via `.env` file
- Secure credential management
- Configurable package ID and date filters

---

## ⚙️ Configuration Requirements

The application requires a `.env` file with the following credentials:

```ini
MACHINE_ID=<RDP Username>
PASSWORD=<RDP Password>
APP_KEY=<RDP App Key>
PACKAGE_ID=<Your Bulk Package ID>
```

---

## 🎮 Program.cs Workflow - Step by Step

The application follows a **detailed 6-step process** with sub-operations:

### **Step 1: 🔐 Initial Authentication**
- Load environment variables from `.env` file
- Extract credentials: `MACHINE_ID`, `PASSWORD`, `APP_KEY`, `PACKAGE_ID`
- Send OAuth2 POST request to `/auth/oauth2/v1/token` endpoint
- Receive access token, refresh token, and expiration time
- Store token information in `Token` object

### **Step 2: 🔍 Query File Sets (Multiple Options)**
- **Step 2A: Query with Date Filter (Recommended)**
  - Use `modifiedSince` parameter to filter files by modification date
  - Set `pageSize=100` to limit results per query
  - Send GET request to `/file-store/v1/file-sets` endpoint
  - Extract the first available FileSet ID from response
  
- **Step 2B: Query All Available Files**
  - List up to 100 FileSet records using bucket name and package ID
  - Handle pagination if more than 100 records exist
  - Check for `@nextLink` in response for additional pages
  - Process subsequent pages using `QueryFileSetPaging()` method

### **Step 3: 📎 Get Actual File URL**
- Use FileSet ID from Step 2 to get cloud storage URL
- Send GET request to `/file-store/v1/files/{fileSet}/stream?doNotRedirect=true`
- Extract actual file URL from response `url` attribute
- URL points to Amazon S3 cloud storage location

### **Step 4: ⬇️ Download TM3 File**
- Parse filename from cloud URL (handle URL encoding)
- Replace `%3A` escape characters with `_` underscores
- Download file bytes using HTTP GET request
- Save file to local directory: `TM3Console\bin\Debug\net8.0\`
- Display download completion message

### **Step 5: 🔄 Token Refresh**  
- Wait 5 seconds before token refresh demonstration
- Send refresh token request to `/auth/oauth2/v1/token` endpoint
- Use `grant_type=refresh_token` with existing refresh token
- Receive new access token and refresh token
- Update token information for continued API access

### **Step 6: 🚪 Session Cleanup**
- Wait 5 seconds before token revocation
- Send POST request to `/auth/oauth2/v1/revoke` endpoint
- Use Basic Authentication with App Key
- Include current access token in request payload
- Invalidate token to end session securely

### **📋 Key Technical Details:**
- **Error Handling**: Each step includes try-catch blocks for HTTP and general exceptions
- **Rate Limiting**: Built-in delays prevent API flooding
- **Pagination**: Automatic handling of large result sets (100 records per page)
- **URL Encoding**: Proper handling of special characters in filenames
- **Security**: Tokens are properly managed and revoked after use

---

## 📁 Output Location

Downloaded TM3 files are saved to:
```
TM3Console\bin\Debug\net8.0\
```

---

## 🛠️ Development Environment

- **IDE**: Visual Studio 2022 (recommended)
- **Alternative**: .NET CLI with any text editor
- **Requirements**: .NET SDK 8.0, Internet connection
- **Platform**: Windows, macOS, Linux

---

## 📜 License

This project is licensed under the **Apache License 2.0** - see `LICENSE.md` for details.

---

## 🏢 Organization

- **Repository**: Example.TM3.RDP.CSharp.Workflow
- **Owner**: LSEG-API-Samples
- **Branch**: main

---

## ⚠️ Important Notes

- **Example Code Only**: Provided "AS IS" for demonstration purposes
- **Not Production Ready**: Requires optimization for production use
- **Credentials Required**: Must have valid RDP account and TM3 package access
- **Rate Limiting**: Avoid flushing requests to prevent API throttling

---

*For detailed setup instructions and API documentation, refer to the `README.md` file.*