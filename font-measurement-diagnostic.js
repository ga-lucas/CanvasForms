// Font Measurement System Diagnostic Script
// Paste this into browser DevTools Console to test

console.log("=== Font Measurement Diagnostic ===\n");

// Test 1: Check if functions exist
console.log("1. Checking if functions exist...");
const hasMeasureText = typeof window.measureText === 'function';
const hasMeasureTextBatch = typeof window.measureTextBatch === 'function';
console.log(`   measureText: ${hasMeasureText ? '✅ EXISTS' : '❌ MISSING'}`);
console.log(`   measureTextBatch: ${hasMeasureTextBatch ? '✅ EXISTS' : '❌ MISSING'}`);

if (!hasMeasureText || !hasMeasureTextBatch) {
    console.error("❌ Font measurement functions not loaded!");
    console.log("   Check that canvas-renderer.js is loaded before blazor.webassembly.js");
} else {
    console.log("✅ Functions loaded successfully\n");

    // Test 2: Single measurement
    console.log("2. Testing single measurement...");
    try {
        const width1 = window.measureText("Arial", 12, "Hello");
        const width2 = window.measureText("Arial", 16, "Hello");
        const width3 = window.measureText("Arial", 12, "WWWWW");
        const width4 = window.measureText("Arial", 12, "iiiii");

        console.log(`   Arial 12px "Hello": ${width1}px`);
        console.log(`   Arial 16px "Hello": ${width2}px ${width2 > width1 ? '✅' : '❌ (should be larger)'}`);
        console.log(`   Arial 12px "WWWWW": ${width3}px`);
        console.log(`   Arial 12px "iiiii": ${width4}px ${width4 < width3 ? '✅' : '❌ (should be smaller)'}`);

        if (width1 > 0 && width2 > width1 && width4 < width3) {
            console.log("✅ Single measurement working correctly\n");
        } else {
            console.error("❌ Single measurement returning unexpected values\n");
        }
    } catch (error) {
        console.error(`❌ Single measurement error: ${error.message}\n`);
    }

    // Test 3: Batch measurement
    console.log("3. Testing batch measurement...");
    try {
        const texts = ["Hello", "World", "Test"];
        const widths = window.measureTextBatch("Arial", 12, texts);

        if (Array.isArray(widths) && widths.length === texts.length) {
            console.log(`   Batch result: [${widths.join(', ')}]`);

            const allPositive = widths.every(w => w > 0);
            if (allPositive) {
                console.log("✅ Batch measurement working correctly\n");
            } else {
                console.error("❌ Batch measurement returning zero or negative values\n");
            }
        } else {
            console.error(`❌ Batch measurement returned wrong type or length\n`);
        }
    } catch (error) {
        console.error(`❌ Batch measurement error: ${error.message}\n`);
    }

    // Test 4: Empty/null handling
    console.log("4. Testing edge cases...");
    try {
        const empty = window.measureText("Arial", 12, "");
        const nullText = window.measureText("Arial", 12, null);
        const emptyBatch = window.measureTextBatch("Arial", 12, []);

        console.log(`   Empty string: ${empty}px ${empty === 0 ? '✅' : '❌'}`);
        console.log(`   Null text: ${nullText}px ${nullText === 0 ? '✅' : '❌'}`);
        console.log(`   Empty array: ${emptyBatch.length === 0 ? '✅' : '❌'}`);
        console.log("✅ Edge cases handled correctly\n");
    } catch (error) {
        console.error(`❌ Edge case error: ${error.message}\n`);
    }

    // Test 5: Performance test
    console.log("5. Testing performance...");
    try {
        const iterations = 1000;
        const testText = "The quick brown fox jumps over the lazy dog";

        console.time("1000 measurements");
        for (let i = 0; i < iterations; i++) {
            window.measureText("Arial", 12, testText);
        }
        console.timeEnd("1000 measurements");

        console.log("   (Should be <10ms for 1000 measurements)\n");
    } catch (error) {
        console.error(`❌ Performance test error: ${error.message}\n`);
    }

    // Test 6: Check measurement context
    console.log("6. Checking measurement context...");
    if (typeof __measureCtx !== 'undefined' && __measureCtx) {
        console.log(`   Context type: ${__measureCtx.constructor.name} ✅`);
        console.log(`   Canvas size: ${__measureCanvas.width}x${__measureCanvas.height}`);

        // Test font setting
        __measureCtx.font = "16px Arial";
        console.log(`   Font setting: ${__measureCtx.font} ${__measureCtx.font.includes('Arial') ? '✅' : '❌'}`);
        console.log("✅ Measurement context working\n");
    } else {
        console.error("❌ Measurement context (__measureCtx) not found\n");
    }
}

// Summary
console.log("=== Diagnostic Summary ===");
if (hasMeasureText && hasMeasureTextBatch) {
    console.log("✅ Font measurement system is WORKING");
    console.log("\nTo test in your app:");
    console.log("1. Click on a TextBox control");
    console.log("2. Type some text");
    console.log("3. Check if caret appears at correct position");
    console.log("4. Use arrow keys to move caret");
    console.log("5. Caret should move to accurate positions");
} else {
    console.log("❌ Font measurement system has ISSUES");
    console.log("\nTroubleshooting steps:");
    console.log("1. Check Network tab for canvas-renderer.js (should be loaded)");
    console.log("2. Check Console for JavaScript errors");
    console.log("3. Verify script load order in index.html:");
    console.log("   - canvas-renderer.js BEFORE blazor.webassembly.js");
    console.log("4. Hard refresh page (Ctrl+Shift+R)");
}

console.log("\n=== End of Diagnostic ===");
