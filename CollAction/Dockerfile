FROM microsoft/aspnetcore:1.1.1
ARG source
WORKDIR /app
EXPOSE 80
COPY ${source:-bin/netcoreapp1.1/publish} .
RUN echo "deb http://ftp.debian.org/debian jessie-backports main" >> /etc/apt/sources.list
RUN apt-get update && apt-get install -y python cron openssl
RUN apt-get install -y certbot -t jessie-backports
RUN echo '31 0,12 * * * root certbot-auto renew --rsa-key-size 4096 --staging --webroot /app/wwwroot/ -d $SSL_DOMAIN && openssl pkcs12 -export /app/certificate/cert.pfx -inkey /etc/letsencrypt/live/$SSL_DOMAIN/privkey.pem -in /etc/letsencrypt/live/$SSL_DOMAIN/cert.pem -certfile /etc/letsencrypt/live/$SSL_DOMAIN/chain.pem' >> /etc/crontab
RUN mkdir -p /app/wwwroot/.well-known/acme-challenge
VOLUME ["/app/wwwroot/usercontent/"]
VOLUME ["/app/certificate/"]
ENTRYPOINT ["dotnet", "CollAction.dll"]