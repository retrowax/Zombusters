package com.retrowax.zombusters.game.ui

import korlibs.image.font.DefaultTtfFont
import korlibs.image.font.Font
import korlibs.image.font.readTtfFont
import korlibs.io.file.std.resourcesVfs

/**
 * Centralized font definitions for Zombusters UI.
 *
 * FONT STATUS: SUBSTITUTED
 * Original legacy fonts: Arial-based XNA SpriteFont XNBs compiled by MonoGame.
 *   - Menu\ArialMenuInfo   (body/info text, small)
 *   - Menu\ArialMenuList   (menu list items, medium)
 *   - Menu\ArialMenuHeader (section headers, large)
 *   - Menu\DigitBig        (large numbers, e.g., level number)
 *   - Menu\DigitLow        (small digits/symbols)
 *
 * The .spritefont source XML references "Arial" (Windows system font).
 * No TTF source was bundled in the legacy project.
 * The SpriteFont XNBs are compiled binary and cannot be decoded to TTF.
 *
 * Substitute: Poppins (OFL-licensed, bundled in composeResources/font/).
 *   menuHeader  → poppins_extra_bold.ttf   (large headers, ~ArialMenuHeader)
 *   menuList    → poppins_bold.ttf         (menu items, ~ArialMenuList)
 *   menuInfo    → poppins_regular.ttf      (body text, ~ArialMenuInfo)
 *   digitBig    → poppins_extra_bold.ttf   (large digits, ~DigitBig)
 *
 * If TTF loading fails, KorGE's built-in DefaultTtfFont is used as fallback.
 * Public properties are always non-nullable (Font) for direct use in Text.font.
 */
object ZombustersFonts {

    private var _menuHeader: Font? = null
    private var _menuList: Font? = null
    private var _menuInfo: Font? = null
    private var _digitBig: Font? = null

    val menuHeader: Font get() = _menuHeader ?: DefaultTtfFont
    val menuList: Font get() = _menuList ?: DefaultTtfFont
    val menuInfo: Font get() = _menuInfo ?: DefaultTtfFont
    val digitBig: Font get() = _digitBig ?: DefaultTtfFont

    private var loaded = false

    suspend fun loadFrom(fontResourcesPath: String) {
        if (loaded) return
        if (fontResourcesPath.isEmpty()) return
        try {
            _menuHeader = resourcesVfs["$fontResourcesPath/poppins_extra_bold.ttf"].readTtfFont()
            _menuList   = resourcesVfs["$fontResourcesPath/poppins_bold.ttf"].readTtfFont()
            _menuInfo   = resourcesVfs["$fontResourcesPath/poppins_regular.ttf"].readTtfFont()
            _digitBig   = resourcesVfs["$fontResourcesPath/poppins_extra_bold.ttf"].readTtfFont()
            loaded = true
        } catch (_: Exception) {
            // Font load failed — DefaultTtfFont used throughout
        }
    }

    /** Reset for fresh KorGE session. */
    fun reset() {
        loaded = false
        _menuHeader = null; _menuList = null; _menuInfo = null; _digitBig = null
    }
}
