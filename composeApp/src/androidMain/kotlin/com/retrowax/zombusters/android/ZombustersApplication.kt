package com.retrowax.zombusters.android

import android.app.Application
import com.retrowax.zombusters.android.core.di.DependencyContainer

class ZombustersApplication : Application() {

    override fun onCreate() {
        super.onCreate()
        initDependencyContainer()
    }

    private fun initDependencyContainer() {
        DependencyContainer.initialize(this)
    }
}
