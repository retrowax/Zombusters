package com.retrowax.zombusters.di

import com.russhwolf.settings.Settings
import com.retrowax.zombusters.localization.getCurrentLocalization
import org.koin.core.module.dsl.singleOf
import org.koin.dsl.module

val commonModule = module {
    singleOf(::Settings)
    single { getCurrentLocalization() }
}
