# will generate certs for use in https server, you should update the password
openssl req -x509 -newkey rsa:2048 -keyout keytmp.pem -out cert.pem -days 365 -passout pass:HereIsMySuperPass -subj /C=US/ST=Washington/L=Seattle
openssl rsa -in keytmp.pem -out key.pem -passin pass:HereIsMySuperPass
