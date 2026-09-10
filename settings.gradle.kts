pluginManagement {
    repositories {
        google()
        gradlePluginPortal()
        maven {
            name = "Central Portal Snapshots"
            url = uri("https://central.sonatype.com/repository/maven-snapshots/")
            content {
                // Only consume org.korge.korlibs snapshots
                includeGroup("org.korge.korlibs")
                includeGroup("org.korge.engine")
                includeGroup("org.korge.gradleplugins")
            }
        }
        maven {
            name = "OSSRH Snapshots"
            url = uri("https://oss.sonatype.org/content/repositories/snapshots/")
            content {
                includeGroup("org.korge.korlibs")
                includeGroup("org.korge.engine")
                includeGroup("org.korge.gradleplugins")
            }
        }
        mavenCentral()
    }
}
plugins {
    id("org.gradle.toolchains.foojay-resolver-convention") version "1.0.0"
}

dependencyResolutionManagement {
    repositories {
        mavenLocal()
        google()
        maven {
            name = "Central Portal Snapshots"
            url = uri("https://central.sonatype.com/repository/maven-snapshots/")
            content {
                // Only consume org.korge.korlibs snapshots
                includeGroup("org.korge.korlibs")
                includeGroup("org.korge.engine")
                includeGroup("org.korge.gradleplugins")
            }
        }
        maven {
            name = "OSSRH Snapshots"
            url = uri("https://oss.sonatype.org/content/repositories/snapshots/")
            content {
                includeGroup("org.korge.korlibs")
                includeGroup("org.korge.engine")
                includeGroup("org.korge.gradleplugins")
            }
        }
        mavenCentral()
        maven("https://androidx.dev/storage/compose-compiler/repository")
        maven("https://maven.pkg.jetbrains.space/public/p/compose/dev")
        maven("https://maven.pkg.jetbrains.space/public/p/ktor/eap")
        maven("https://s01.oss.sonatype.org/content/repositories/releases/")
    }

    versionCatalogs {
        create("libs")
    }
}

rootProject.name = "SteelVectors"
include(":composeApp")
include(":shared")
