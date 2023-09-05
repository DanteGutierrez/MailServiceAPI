CLS

docker stop mail-api-1
docker stop mail-api-2

docker rm mail-api-1
docker rm mail-api-2

docker rmi mail-api:1

docker build -t mail-api:1 .
docker run -d -p 9010:80 --name mail-api-1 -d --net netCSC380 mail-api:1
docker run -d -p 9020:80 --name mail-api-2 -d --net netCSC380 mail-api:1