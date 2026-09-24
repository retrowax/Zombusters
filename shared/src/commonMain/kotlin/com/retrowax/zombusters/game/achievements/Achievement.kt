package com.retrowax.zombusters.game.achievements

enum class Achievement(val key: String) {
    DODGER("dodger"),          // Complete a level without taking damage
    UNSTOPPABLE("unstoppable"), // 25+ kill streak without dying
    QUICK_DEAD("quickdead"),   // Kill 10+ enemies in 10 seconds
    EAGLE_EYE("eagleeye")      // 80%+ bullet accuracy in a level
}
