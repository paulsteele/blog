FROM hugomods/hugo:reg-node-lts-0.131.0 AS hugo

FROM nginx
COPY --from=hugo /target /usr/share/nginx/html