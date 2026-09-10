package com.retrowax.zombusters.platform

import com.retrowax.zombusters.di.appModule
import org.koin.core.context.startKoin

fun initKoin() {
    startKoin {
        modules(appModule())
    }
}
