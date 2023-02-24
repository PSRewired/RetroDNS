# RetroDNS
A simple DNS proxy server to help people who cannot connect to third-party DNS servers

## Usage
### Adding domain entries
Edit [domains.txt](RetroDNS/domains.txt) and add any records that require a proxy.
By default, domains are split into wildcards until they are funneled down to a single "catchall" route.
Ex:
```text
api.psrewired.com
*.psrewired.com
*
```
which would allow you to define wildcards to any domain/subdomain for convenience.

### Running the app
- Configure your PC IP in the settings or let it auto-discover (TODO) and run the application
- Define any domains via the instructions above
- Run the application
- Point your console/PC/Emulator to your PC's IP address
- Profit
