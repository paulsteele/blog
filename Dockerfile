FROM ruby:2.7.2-buster as builder

# Install program to configure locales
# if error on host run echo N | sudo tee /sys/module/overlay/parameters/metacopy
RUN apt-get update && apt-get install -y locales
RUN dpkg-reconfigure locales && \
  locale-gen C.UTF-8 && \
  /usr/sbin/update-locale LANG=C.UTF-8

# Install needed default locale for Makefly
RUN echo 'en_US.UTF-8 UTF-8' >> /etc/locale.gen && \
  locale-gen

# Set default locale for the environment
ENV LC_ALL C.UTF-8
ENV LANG en_US.UTF-8
ENV LANGUAGE en_US.UTF-8

WORKDIR /blog

RUN gem install bundler:1.17.1

COPY Gemfile Gemfile
COPY Gemfile.lock Gemfile.lock

RUN bundle install

COPY . .

RUN bundle exec jekyll build

FROM nginx as site 

COPY --from=builder /blog/_site /usr/share/nginx/html
