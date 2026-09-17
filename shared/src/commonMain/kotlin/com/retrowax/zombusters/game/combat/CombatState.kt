package com.retrowax.zombusters.game.combat

class CombatState {
    val bullets: MutableList<Projectile> = mutableListOf()
    val shotgunShells: MutableList<ShotgunShell> = mutableListOf()

    fun clearAll() {
        bullets.clear()
        shotgunShells.clear()
    }
}
