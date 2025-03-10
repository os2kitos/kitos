#!/bin/bash

cd ../..
if [[ "$(uname)" == "Linux" ]]; then
	sudo docker build -t kitos-pubsub -f PubSub.Application/Dockerfile .
else
	docker build -t kitos-pubsub -f PubSub.Application/Dockerfile .
fi