#!/bin/bash

DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )"

cd "$DIR/../deploy/"

rm -rf *

mkdir -p "./FxSchoolCalculus/"
cp -rf "$DIR/../FxSchoolCalculus/bin/Release/." "./FxSchoolCalculus/"
cp -rf "$DIR"/../win/* "./FxSchoolCalculus/"

mkdir -p "./FxSchoolDictate/"
cp -rf "$DIR/../FxSchoolDictate/bin/Release/." "./FxSchoolDictate/"
cp -rf "$DIR"/../win/* "./FxSchoolDictate/"

zip -r "./FxSchool.zip" *
