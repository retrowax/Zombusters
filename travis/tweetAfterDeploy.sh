#!/bin/bash

if [[ $TRAVIS_TAG ]]
  python ./twitterUpdate.py -k "$TWITTER_API_KEY" -s "$TWITTER_API_SECRET" -a "$TWITTER_ACCESS_TOKEN" -t "$TWITTER_ACCESS_TOKEN_SECRET" -u "New Zombusters version $MAJOR.$MINOR.$PATCH now available for download. Check out the details at https://retrowax.itch.io/zombusters"
else
  python ./twitterUpdate.py -k "$TWITTER_API_KEY" -s "$TWITTER_API_SECRET" -a "$TWITTER_ACCESS_TOKEN" -t "$TWITTER_ACCESS_TOKEN_SECRET" -u "New Zombusters BETA version $MAJOR.$MINOR.$PATCH now available for download. Check out the details at https://github.com/retrowax/Zombusters/releases"
fi
