# RetroDNS
A simple DNS proxy server to help people who cannot connect to third-party DNS servers

## Usage
### Adding domain entries
Edit [99-domains.txt](99-domains.txt) and add any records that require a proxy.
By default, domains are split into wildcards until they are funneled down to a single "catchall" route.

If needed, you may specify multiple domain files. They are loaded in priority based on the optional number appended to the beginning
of the filename. (Ex: 00-domains.txt will load before any other file)

**Note: The names of these files may post-fixed with any designator that you choose (Ex: 05-domains_openspy.txt)**
Ex:
```text
# Example to respond with an explicit IP address
api.psrewired.com=ip://129.168.0.123 

# Example to forward PSRewired sub-domains to their DNS server
*.psrewired.com=dns://67.222.156.250

# Example to foward all other requests to Google DNS
*=dns://8.8.8.8
```
which would allow you to define wildcards to any domain/subdomain for convenience.

### Running the app via the command line
- Define any domains via the instructions above
- Point your console/PC/Emulator to your PC's IP address
- Run RetroDNS with the argument specifying your PC's IP address. (Ex: ./RetroDNS 192.168.1.123)

### Running the UI
- Define any domains via the instructions above
- Launch the `RetroDNS.UI` application
- Select your PC's network interface
- Point your console/PC/Emulator to the IP displayed
- Hit "Start"
- Optionally, check the logs tab to make sure that requests are being sent to the RetroDNS server

** If you need to make domain changes, simply press "Stop", edit the domain file and then press "Start" again. The file will automatically be reloaded**
