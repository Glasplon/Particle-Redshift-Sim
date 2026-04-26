#!/bin/bash
#chmod +x render_and_encode.sh

set -e  # exit if something fails

FRAME_COUNT=120
FPS=30
OUTPUT_DIR="outputData"
OUTPUT_VIDEO="output.mp4"

mkdir -p "$OUTPUT_DIR"

echo "Generating frames..."

for ((i=1; i<=FRAME_COUNT; i++))
do
    echo "Frame $i"
    dotnet run "$i" s
done

echo "Encoding video..."

ffmpeg -y -framerate $FPS -i "$OUTPUT_DIR/%d.png" \
    -c:v libx264 -pix_fmt yuv420p "$OUTPUT_VIDEO"

echo "Done: $OUTPUT_VIDEO"