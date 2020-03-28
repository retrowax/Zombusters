#!/bin/bash

if [ $TRAVIS_TAG ]; then
  python ./twitterUpdate.py -k "$TWITTER_API_KEY" -s "$TWITTER_API_SECRET" -a "$TWITTER_ACCESS_TOKEN" -t "$TWITTER_ACCESS_TOKEN_SECRET" -u "New #Zombusters version $MAJOR.$MINOR.$PATCH available! Click the link to #download it. Enjoy! #opensource #itchio #macos #linux #windows #free https://retrowax.itch.io/zombusters"
else
  python ./twitterUpdate.py -k "$TWITTER_API_KEY" -s "$TWITTER_API_SECRET" -a "$TWITTER_ACCESS_TOKEN" -t "$TWITTER_ACCESS_TOKEN_SECRET" -u "#Zombusters BETA v$MAJOR.$MINOR.$PATCH-$SNAPSHOT_NOW available for #download now. #opensource #itchio #macos #linux #windows #free Check out the details at https://github.com/retrowax/Zombusters-Enhanced-Edition/commits/master"
fi
