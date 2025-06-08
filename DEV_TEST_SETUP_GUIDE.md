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
  winget install --id Python.Python.3 --source winget
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

Install FastMCP for local testing:

```sh
pip install fastmcp
```

---

## 5. Additional Notes

- Depending on your system, you may need to run some commands as administrator.
- For more information on forking repositories, see [GitHub’s guide](https://docs.github.com/en/get-started/quickstart/fork-a-repo).

---

Happy contributing!