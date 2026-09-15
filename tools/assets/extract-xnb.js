#!/usr/bin/env node
/**
 * XNB Texture2D extractor for XNA 4.0 Windows format.
 * Reads .xnb files compressed with LZX and exports as PNG.
 *
 * Usage:
 *   node extract-xnb.js <input.xnb> <output.png>
 *   node extract-xnb.js --all <content_dir> <output_dir>
 */

'use strict';

const xnb = require('xnb');
const { PNG } = require('pngjs');
const fs = require('fs');
const path = require('path');

function xnbToPng(inputPath) {
    const nodeBuf = fs.readFileSync(inputPath);
    const buf = nodeBuf.buffer.slice(nodeBuf.byteOffset, nodeBuf.byteOffset + nodeBuf.byteLength);
    const result = xnb.bufferToXnb(buf);

    const content = result.content;
    if (!content || !content.export || content.export.type !== 'Texture2D') {
        return null; // not a texture
    }

    const dataObj = content.export.data;
    const allJson = JSON.stringify(result);
    const widthMatch = allJson.match(/"width":(\d+)/);
    const heightMatch = allJson.match(/"height":(\d+)/);
    if (!widthMatch || !heightMatch) {
        return null;
    }
    const width = parseInt(widthMatch[1]);
    const height = parseInt(heightMatch[1]);

    // Convert data object (numeric keys -> byte values) to Buffer
    const numBytes = Object.keys(dataObj).length;
    const pixelBuf = Buffer.alloc(numBytes);
    for (let i = 0; i < numBytes; i++) {
        pixelBuf[i] = dataObj[i] || 0;
    }

    // XNA SurfaceFormat.Color = BGRA8 stored as RGBA8 in memory
    // Swap R and B channels to get standard RGBA
    const png = new PNG({ width, height, colorType: 6, inputColorType: 6 });
    png.data = Buffer.alloc(width * height * 4);
    for (let i = 0; i < width * height; i++) {
        const r = pixelBuf[i * 4 + 0];
        const g = pixelBuf[i * 4 + 1];
        const b = pixelBuf[i * 4 + 2];
        const a = pixelBuf[i * 4 + 3];
        // XNA format=0 is BGRA on disk; swap to RGBA for PNG
        png.data[i * 4 + 0] = b;
        png.data[i * 4 + 1] = g;
        png.data[i * 4 + 2] = r;
        png.data[i * 4 + 3] = a;
    }

    return { png, width, height };
}

function savePng(pngData, outputPath) {
    const buf = PNG.sync.write(pngData.png);
    fs.writeFileSync(outputPath, buf);
}

function extractOne(inputXnb, outputPng) {
    process.stderr.write = () => {}; // suppress xnb verbose logs on stderr
    const origLog = console.log.bind(console);
    console.log = () => {}; // suppress xnb verbose logs

    let result;
    try {
        result = xnbToPng(inputXnb);
    } catch (e) {
        console.log = origLog;
        process.stderr.write = process.stderr.write;
        return { success: false, error: e.message };
    }
    console.log = origLog;

    if (!result) {
        return { success: false, error: 'not a Texture2D or missing dimensions' };
    }

    fs.mkdirSync(path.dirname(outputPng), { recursive: true });
    savePng(result, outputPng);
    return { success: true, width: result.width, height: result.height };
}

function extractAll(contentDir, outputDir) {
    const xnbFiles = [];
    function walk(dir) {
        for (const entry of fs.readdirSync(dir, { withFileTypes: true })) {
            const full = path.join(dir, entry.name);
            if (entry.isDirectory()) walk(full);
            else if (entry.name.endsWith('.xnb')) xnbFiles.push(full);
        }
    }
    walk(contentDir);

    let succeeded = 0, failed = 0, skipped = 0;
    const log = [];

    for (const xnbPath of xnbFiles) {
        const rel = path.relative(contentDir, xnbPath);
        const outRel = rel.replace(/\.xnb$/, '.png');
        const outPath = path.join(outputDir, outRel);

        const res = extractOne(xnbPath, outPath);
        if (res.success) {
            succeeded++;
            log.push({ status: 'OK', file: rel, width: res.width, height: res.height });
            process.stdout.write(`OK  ${rel} (${res.width}x${res.height})\n`);
        } else if (res.error === 'not a Texture2D or missing dimensions') {
            skipped++;
            log.push({ status: 'SKIP', file: rel, reason: res.error });
        } else {
            failed++;
            log.push({ status: 'FAIL', file: rel, error: res.error });
            process.stdout.write(`FAIL ${rel}: ${res.error}\n`);
        }
    }

    process.stdout.write(`\nDone. OK: ${succeeded}, Skipped (not texture): ${skipped}, Failed: ${failed}\n`);
    return log;
}

// Main
const args = process.argv.slice(2);
if (args[0] === '--all') {
    const contentDir = args[1];
    const outputDir = args[2];
    if (!contentDir || !outputDir) {
        console.error('Usage: extract-xnb.js --all <content_dir> <output_dir>');
        process.exit(1);
    }
    const log = extractAll(contentDir, outputDir);
    fs.writeFileSync(path.join(outputDir, 'extraction-log.json'),
        JSON.stringify(log, null, 2));
} else if (args[0] && args[1]) {
    const res = extractOne(args[0], args[1]);
    if (res.success) {
        console.log(`Extracted: ${args[1]} (${res.width}x${res.height})`);
    } else {
        console.error(`Failed: ${res.error}`);
        process.exit(1);
    }
} else {
    console.error('Usage: extract-xnb.js <input.xnb> <output.png>');
    console.error('       extract-xnb.js --all <content_dir> <output_dir>');
    process.exit(1);
}
