FROM hugomods/hugo:base-0.145.0 AS hugo
WORKDIR /src
COPY . .
RUN hugo --minify

FROM nginx
COPY --from=hugo /src/public /usr/share/nginx/html
