.PHONY: docker-build docker-run

DOCKER_REGISTRY   ?= azurecate2ehub-microsoft.azurecr.io
IMAGE_PREFIX      ?= hpc
SHORT_NAME        ?= hpc-robot

TEMPLATES_PATH	= ../../scripts/deploy/kubernetes/

GIT_BRANCH	= ${shell git rev-parse --abbrev-ref HEAD}
GIT_COMMIT	= ${shell git rev-parse HEAD}
GIT_SHA		= ${git rev-parse --short HEAD}

ENV	?= local

.PHONY: clean
clean:
	@rm -rf out bin obj

DOCKER_VERSION	= $(subst /,-,${GIT_BRANCH}).${GIT_COMMIT}
IMAGE			= ${DOCKER_REGISTRY}/${IMAGE_PREFIX}/${SHORT_NAME}:${DOCKER_VERSION}
CONTAINER_NAME	= hpc-robot

.PHONY: docker-build
docker-build:
	@docker build --rm -t ${IMAGE} .

.PHONY: docker-run
docker-run:
	@docker run -d --name ${CONTAINER_NAME} \
	    --mount type=bind,source=$(shell pwd)/Settings,target=/app/settings,readonly \
	    --mount type=bind,source=$(shell pwd)/log,target=/var/log/hpc \
	    ${IMAGE}

.PHONY: docker-clean
docker-clean:
	@docker stop ${CONTAINER_NAME} || true && docker rm ${CONTAINER_NAME} || true

.PHONY: docker-all
docker-all: docker-build docker-clean docker-run

.PHONY: docker-push
docker-push:
	@docker push ${IMAGE}

.PHONY: kubernetes-info
kubernetes-info:
	kubectl cluster-info

.PHONY: kubernetes-deploy
kubernetes-deploy:
	@mkdir -p ${TEMPLATES_PATH}/environments/${ENV}

	@find ${TEMPLATES_PATH} -maxdepth 1 -type f -name "*.yaml" -print0 | xargs -0 -I{} sh -c \
		'sed "s#{{environment}}#${ENV}#; s#{{image}}#${IMAGE}#" "$$1" > ${TEMPLATES_PATH}/environments/${ENV}/"$$(basename $$1)"' -- {}

	@kubectl apply -f ${TEMPLATES_PATH}/environments/${ENV}/namespace.yaml
	@kubectl apply -f ${TEMPLATES_PATH}/environments/${ENV}/config.yaml
	@kubectl apply -f ${TEMPLATES_PATH}/environments/${ENV}/deployment.yaml --force

	@kubectl create configmap hpc-robot -n hpc-${ENV} \
	    --from-file=Settings/appsettings.json \
	    --from-file=Settings/appsettings.${ENV}.json \
	    --from-file=Settings/robot.parameters.tpl  \
	    --dry-run -o yaml | kubectl replace configmap hpc-robot -n hpc-${ENV} -f -

.PHONY: deploy
deploy: docker-build docker-push kubernetes-deploy

info:
	 @echo "Git Branch:			${GIT_BRANCH}"
	 @echo "Git Commit:			${GIT_COMMIT}"
	 @echo "Git Tree State:		${GIT_DIRTY}"
	 @echo "Docker Version:		${DOCKER_VERSION}"
	 @echo "Registry:			${DOCKER_REGISTRY}"
	 @echo "Image:				${IMAGE}"
