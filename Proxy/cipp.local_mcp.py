import sys
print("PYTHON EXECUTABLE:", sys.executable, file=sys.stderr)
print("PYTHON VERSION:", sys.version, file=sys.stderr)
from fastmcp import FastMCP

# Create a proxy directly from a config dictionary
config = {
    "mcpServers": {
        "default": {  # For single server configs, 'default' is commonly used
            "url": "http://localhost:3001/mcp",
            "transport": "streamable-http"
        }
    }
}

# Create a proxy to the configured server
proxy = FastMCP.as_proxy(config, name="CIPP-MCP Proxy")

# Target a remote SSE server directly by URL
# proxy = FastMCP.as_proxy("http://localhost:3001/mcp", name="CIPP-MCP Proxy")



# Run the proxy with stdio transport for local access
if __name__ == "__main__":
    proxy.run()