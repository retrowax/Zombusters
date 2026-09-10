import androidx.compose.ui.window.Window
import androidx.compose.ui.window.application
import com.steelvectors.app.MainApp
import com.steelvectors.app.di.commonModule
import com.steelvectors.app.ui.theme.SkyVectorTheme
import org.koin.core.context.startKoin

fun main() = application {
    startKoin {
        modules(commonModule)
    }
    Window(onCloseRequest = ::exitApplication) {
        SkyVectorTheme {
            MainApp()
        }
    }
}
