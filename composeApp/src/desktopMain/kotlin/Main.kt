import androidx.compose.ui.window.Window
import androidx.compose.ui.window.application
import com.retrowax.zombusters.MainApp
import com.retrowax.zombusters.di.commonModule
import com.retrowax.zombusters.ui.theme.ZombustersTheme
import org.koin.core.context.startKoin

fun main() = application {
    startKoin {
        modules(commonModule)
    }
    Window(onCloseRequest = ::exitApplication) {
        ZombustersTheme {
            MainApp()
        }
    }
}
