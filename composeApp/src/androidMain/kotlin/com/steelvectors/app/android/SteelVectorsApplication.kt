package com.steelvectors.app.android

import android.app.Application
import com.steelvectors.app.android.core.di.DependencyContainer

class SteelVectorsApplication : Application() {

    override fun onCreate() {
        super.onCreate()
        initDependencyContainer()
    }

    private fun initDependencyContainer() {
        DependencyContainer.initialize(this)
    }
}
