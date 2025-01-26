FROM mcr.microsoft.com/mssql/server:2022-latest

COPY init.sql /docker-entrypoint-initdb.d/
ENV ACCEPT_EULA=Y
ENV SA_PASSWORD=Med!porta1234

CMD ["/opt/mssql/bin/sqlservr"]
