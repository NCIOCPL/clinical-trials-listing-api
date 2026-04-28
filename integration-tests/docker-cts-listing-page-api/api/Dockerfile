FROM mcr.microsoft.com/dotnet/sdk:8.0
WORKDIR /app

# Get the NIH Root certificates and install them so we work on VPN
# Download from https://ocio.nih.gov/Smartcard/Pages/PKI_chain.aspx
RUN mkdir /usr/local/share/ca-certificates/nih \
    && curl -o /usr/local/share/ca-certificates/nih/NIH-DPKI-ROOT-1A-base64.crt http://nihdpkicrl.nih.gov/certchains/NIH-DPKI-CA-1A%281%29.crt \
    && curl -o /usr/local/share/ca-certificates/nih/NIH-DPKI-CA-1A-base64.crt http://nihdpkicrl.nih.gov/certchains/NIH-DPKI-ROOT-1A.cer \
    && update-ca-certificates

EXPOSE 5001
ENV ASPNETCORE_ENVIRONMENT=inttest
ENV ASPNETCORE_URLS="http://*:5000"
ENV DOTNET_USE_POLLING_FILE_WATCHER=true
ENTRYPOINT ["dotnet", "watch", "--project", "src/NCI.OCPL.Api.CTSListingPages", "run", "--quiet"]
