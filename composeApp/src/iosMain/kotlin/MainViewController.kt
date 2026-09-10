import androidx.compose.ui.window.ComposeUIViewController
import com.retrowax.zombusters.MainApp
import platform.UIKit.UIViewController

fun homeScreenViewController(): UIViewController = ComposeUIViewController {
    MainApp()
}

/* ktlint-disable */
