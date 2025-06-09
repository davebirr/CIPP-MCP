# Development & Testing Setup Guide

Welcome! This guide will help you set up your environment for contributing to the CIPP-MCP project. Please follow the steps below to ensure a smooth development experience.

---

## 1. Required Tools

Install the following tools using [winget](https://learn.microsoft.com/en-us/windows/package-manager/winget/) (run commands in **PowerShell**):

- **Visual Studio Code**
  ```sh
  winget install --exact vscode
  ```

- **PowerShell 7**
  ```sh
  winget install --exact Microsoft.PowerShell
  ```

- **Git**
  ```sh
  winget install --exact Git.Git
  ```

- **Node.js v22.x LTS**
  ```sh
  winget install --exact OpenJS.NodeJS.LTS --version 22.13.0
  winget pin add OpenJS.NodeJS.LTS --version 22.13.* --force
  ```

- **.NET SDK 8**
  ```sh
  winget install --exact Microsoft.DotNet.SDK.8
  ```

- **.NET SDK 9**
  ```sh
  winget install --exact Microsoft.DotNet.SDK.9
  ```

- **Python 3**
  ```sh
  winget install --exact Python.Python.3.13
  ```



---

## 2. Global npm Packages

Some npm packages need to be installed globally. You may need to run these commands as **Administrator** if you encounter permission issues.

```sh
npm install --global azure-functions-core-tools@4 --unsafe-perm true
npm install --global azurite
```

---

## 3. Repository Structure

You’ll need both the CIPP-MCP and CIPP-API repositories as siblings in a parent folder, for example:

```
CIPP-Project/
├── CIPP-MCP/
└── CIPP-API/
```

### Fork the Repositories

- [Fork CIPP-MCP](https://github.com/davebirr/CIPP-MCP)
- [Fork CIPP-API](https://github.com/KelvinTegelaar/CIPP)

Clone your forks into the same parent directory.

> **Tip:**  
> A Git repository is a `.git/` folder inside a project. It tracks all changes made to files in the project. Changes are committed to the repository, building up a history of the project.

---

## 4. Python Dependencies

Install UV - Python package and project manager
Install FastMCP - Python based Framework for local testing of MCP

```sh
pip install uv
pip install fastmcp
```

> **Note:** If you're on Windows ARM64 and building from source is problematic, try installing precompiled wheels:
> ```sh
> pip install --only-binary=:all: fastmcp
> ```

---

## 5. Additional Notes

- Depending on your system, you may need to run some commands as administrator.
- For more information on forking repositories, see [GitHub’s guide](https://docs.github.com/en/get-started/quickstart/fork-a-repo).

---

## 6. Running CIPP-MCP as a Streamable HTTP MCP Endpoint

CIPP-MCP runs as a Streamable HTTP MCP endpoint (remote server). This allows integration with clients that support HTTP-based MCP communication.

However, some clients—such as Claude—only communicate via `stdio` (standard input/output) and do not support HTTP endpoints directly. To enable local development and testing with these clients, a FastMCP Proxy is provided in the `Proxy` folder of this project.

### Using the FastMCP Proxy

1. Open a terminal and navigate to the `Proxy` folder in this repository.
2. Make sure you can start the proxy with the following command:
   ```sh
   python .\cipp.local_mcp.py
   ```
3. Press Ctrl-C to exit. Claude will invoke the proxy when it starts.

### Integrating with Claude Desktop via FastMCP Proxy

To use CIPP-MCP with Claude Desktop (or other stdio-based MCP clients), you need to configure Claude to launch the FastMCP Proxy as a custom MCP server.

1. **Install Claude Desktop** if you haven't already.
2. **Edit your `claude_desktop_config.json`** file to add a new MCP server entry that launches the FastMCP Proxy. You can get to the file via File -> Settings -> Developer -> Edit Config.

Example configuration:

```json
{
    "mcpServers": {
        "CIPP-MCP Proxy": {
            "command": "uv",
            "args": [
                "run",
                "python",
                "C:/path/to/your/CIPP-Project/CIPP-MCP/Proxy/cipp.local_mcp.py"
            ]
        }
    }
}
```

- Replace `C:/path/to/your/CIPP-Project/CIPP-MCP/Proxy` with the actual path to your Proxy folder.
- Save the file and restart Claude Desktop.

Claude will now use the FastMCP Proxy to communicate with the CIPP-MCP HTTP endpoint, allowing you to test and debug locally.

---
Happy contributing!