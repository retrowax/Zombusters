import org.jetbrains.compose.desktop.application.dsl.TargetFormat
import org.jetbrains.kotlin.gradle.ExperimentalKotlinGradlePluginApi
import org.jetbrains.kotlin.gradle.dsl.JvmTarget

plugins {
    alias(libs.plugins.kotlinMultiplatform)
    alias(libs.plugins.android.application)
    alias(libs.plugins.compose.multiplatform)
    alias(libs.plugins.compose.compiler)
    id("com.google.gms.google-services")
    id("com.google.firebase.crashlytics")
}

// Prevent skiko-awt (desktop JVM) from leaking into Android compilation classpath
configurations.configureEach {
    if (name.contains("android", ignoreCase = true)) {
        exclude(group = "org.jetbrains.skiko", module = "skiko-awt")
    }
}

val versionNum: String? = project.findProperty("versionNum") as String?
val versionMajor = project.findProperty("zombusters.version.major")?.toString()?.toIntOrNull() ?: 1
val versionMinor = project.findProperty("zombusters.version.minor")?.toString()?.toIntOrNull() ?: 0
val versionPatch = project.findProperty("zombusters.version.patch")?.toString()?.toIntOrNull() ?: 0

fun versionCode(): Int {
    versionNum?.let {
        return (versionMajor * 1000000) + (versionMinor * 1000) + it.toInt()
    } ?: return versionMinor + 1
}

kotlin {
    /*@OptIn(ExperimentalWasmDsl::class)
    wasmJs {
        moduleName = "composeApp"
        browser {
            commonWebpackConfig {
                outputFileName = "composeApp.js"
                devServer = (devServer ?: KotlinWebpackConfig.DevServer()).apply {
                    static = (static ?: mutableListOf()).apply {
                        // Serve sources to debug inside browser
                        add(project.projectDir.path)
                    }
                }
            }
        }
        binaries.executable()
    }*/
    androidTarget {
        @OptIn(ExperimentalKotlinGradlePluginApi::class)
        compilerOptions {
            jvmTarget.set(JvmTarget.JVM_17)
        }
    }
    jvm("desktop")
    listOf(
        iosArm64(),
        iosSimulatorArm64()
    ).forEach { iosTarget ->
        iosTarget.binaries.framework {
            baseName = "ComposeApp"
            isStatic = true
        }
    }
    sourceSets {
        val desktopMain = getByName("desktopMain")
        androidMain.dependencies {
            implementation(compose.preview)
            implementation(libs.androidx.activity.compose)
        }
        commonMain.dependencies {
            implementation(compose.runtime)
            implementation(compose.foundation)
            implementation(compose.material)
            implementation(compose.ui)
            implementation(compose.components.resources)
            implementation(compose.components.uiToolingPreview)
            implementation(project(":shared"))

            // Dependency Injection
            implementation(libs.koin.core)
            implementation(libs.koin.test)
            implementation(libs.koin.compose)
        }
        desktopMain.dependencies {
            implementation(compose.desktop.currentOs)
            implementation(libs.skiko)
            implementation(compose.ui)
            implementation(compose.foundation)
            implementation(compose.material)
            implementation(compose.material3)
            implementation(compose.animation)
            implementation(compose.materialIconsExtended)
            @OptIn(org.jetbrains.compose.ExperimentalComposeLibrary::class)
            implementation(compose.components.resources)
            implementation(libs.skiko.macos.arm64)

            implementation(libs.androidx.compose.ui.util)

            implementation(libs.kotlinx.serialization.json)
            implementation(libs.kotlinx.datetime)
            implementation(libs.kotlinx.coroutines.core)
            implementation(libs.kotlinx.coroutines.swingui)

            // Kamel for image loading
            implementation(libs.kamel)

            // Multiplatform Settings to encrypted key-value data
            implementation(libs.multiplatform.settings.no.arg)
            implementation(libs.multiplatform.settings.serialization)

            // Dependency Injection
            implementation(libs.koin.core)
            implementation(libs.koin.test)
            implementation(libs.koin.compose)

            // Logging
            implementation(libs.kermit)
        }
    }
}

android {
    namespace = "com.retrowax.zombusters.android"
    compileSdk = libs.versions.android.compileSdk.get().toInt()

    sourceSets["main"].manifest.srcFile("src/androidMain/AndroidManifest.xml")
    sourceSets["main"].res.srcDirs("src/androidMain/res")
    sourceSets["main"].resources.srcDirs("src/commonMain/resources")

    defaultConfig {
        applicationId = "com.retrowax.zombusters.android"
        minSdk = libs.versions.android.minSdk.get().toInt()
        targetSdk = libs.versions.android.targetSdk.get().toInt()
        versionCode = versionCode()
        versionName = "$versionMajor.$versionMinor.$versionPatch"
    }
    packaging {
        resources {
            excludes += "/META-INF/{AL2.0,LGPL2.1}"
        }
    }
    buildTypes {
        getByName("release") {
            isMinifyEnabled = true
            isShrinkResources = true
            proguardFiles(
                getDefaultProguardFile("proguard-android-optimize.txt"),
                "proguard-rules.pro"
            )
            signingConfig = signingConfigs.getByName("debug")
        }
    }
    compileOptions {
        sourceCompatibility = JavaVersion.VERSION_17
        targetCompatibility = JavaVersion.VERSION_17
    }
    buildFeatures {
        compose = true
        buildConfig = true
    }
    dependencies {
        implementation(project(":shared"))

        implementation(platform(libs.compose.bom))
        implementation(platform(libs.firebase.bom))

        implementation(libs.androidx.core.ktx)
        implementation(libs.androidx.core.splashscreen)
        implementation(libs.androidx.tracing.ktx)

        implementation(libs.androidx.ui)
        implementation(libs.androidx.material)
        implementation(libs.androidx.material3)
        implementation(libs.androidx.material3.window.size)
        implementation(libs.androidx.ui.tooling.preview)

        implementation(libs.androidx.activity.compose)
        implementation(libs.accompanist.systemuicontroller)

        implementation(libs.koin.core)
        implementation(libs.koin.android)

        implementation(libs.firebase.crashlytics)
        implementation(libs.firebase.analytics)

        testImplementation(libs.junit)
        testImplementation(libs.koin.test)
        androidTestImplementation(libs.androidx.junit)
        androidTestImplementation(libs.androidx.espresso.core)

        // androidTestImplementation("androidx.compose.ui:ui-test-junit4")
        debugImplementation(libs.androidx.ui.tooling)
    }
}

// Korge's AWT OpenGL backend on macOS requires access to JDK-internal modules blocked by Java 21's module system.
// This mirrors the full set from korlibs.korge.jvmAddOpensList plus exports needed for reflection.
val korgeDesktopJvmArgs = listOf(
    "--add-opens=java.desktop/sun.java2d.opengl=ALL-UNNAMED",
    "--add-exports=java.desktop/sun.java2d.opengl=ALL-UNNAMED",
    "--add-opens=java.desktop/java.awt=ALL-UNNAMED",
    "--add-exports=java.desktop/java.awt=ALL-UNNAMED",
    "--add-opens=java.desktop/sun.awt=ALL-UNNAMED",
    "--add-exports=java.desktop/sun.awt=ALL-UNNAMED",
    "--add-opens=java.desktop/sun.lwawt=ALL-UNNAMED",
    "--add-exports=java.desktop/sun.lwawt=ALL-UNNAMED",
    "--add-opens=java.desktop/sun.lwawt.macosx=ALL-UNNAMED",
    "--add-exports=java.desktop/sun.lwawt.macosx=ALL-UNNAMED",
    "--add-opens=java.desktop/com.apple.eawt=ALL-UNNAMED",
    "--add-opens=java.desktop/com.apple.eawt.event=ALL-UNNAMED",
    // Enable Java2D OpenGL pipeline so OGLUtilities.invokeWithOGLContextCurrent works
    "-Dsun.java2d.opengl=true",
    // Disable coroutine stack-trace recovery so exceptions from Korge show their full native stack
    "-Dkotlinx.coroutines.stacktrace.recovery=false",
)

compose.desktop {
    application {
        mainClass = "MainKt"
        jvmArgs += korgeDesktopJvmArgs

        nativeDistributions {
            targetFormats(TargetFormat.Dmg, TargetFormat.Msi, TargetFormat.Deb)
            packageName = "com.retrowax.zombusters"
            packageVersion = "$versionMajor.$versionMinor.$versionPatch"
        }
    }

    dependencies {
        implementation(compose.desktop.currentOs)
        implementation(libs.skiko)
        implementation(compose.ui)
        implementation(compose.foundation)
        implementation(compose.material)
        implementation(compose.material3)
        implementation(compose.animation)
        implementation(compose.materialIconsExtended)
        @OptIn(org.jetbrains.compose.ExperimentalComposeLibrary::class)
        implementation(compose.components.resources)
        implementation(libs.skiko.macos.arm64)
        implementation(project(":shared"))

        implementation(libs.androidx.compose.ui.util)

        implementation(libs.kotlinx.serialization.json)
        implementation(libs.kotlinx.datetime)
        implementation(libs.kotlinx.coroutines.core)
        implementation(libs.kotlinx.coroutines.swingui)

        // Kamel for image loading
        implementation(libs.kamel)

        // Multiplatform Settings to encrypted key-value data
        implementation(libs.multiplatform.settings.no.arg)
        implementation(libs.multiplatform.settings.serialization)

        // Dependency Injection
        implementation(libs.koin.core)
        implementation(libs.koin.test)
        implementation(libs.koin.compose)

        // Logging
        implementation(libs.kermit)
    }
}

tasks.withType<JavaExec>().configureEach {
    if (name == "hotRunDesktop" || name == "hotDevDesktop" || name == "run" || name == "desktopRun") {
        jvmArgs(korgeDesktopJvmArgs)
    }
}

