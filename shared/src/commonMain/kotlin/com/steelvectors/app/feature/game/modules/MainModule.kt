package com.steelvectors.app.feature.game.modules

/*
class MainModule(val context: Context) : Module() {
  override val title: String = "Caza Espacial"
  override val icon: String = "game/green_04.png"
  override val size: SizeInt = SizeInt(320, 480)
  override val mainScene = MainScene::class
  private val mainSceneObject = MainScene(context)

  override suspend fun AsyncInjector.configure() {
    mapPrototype { mainSceneObject }
  }

  fun onResume() {
    mainSceneObject.resume()
  }

  fun onPause() {
    mainSceneObject.pause()
  }

  fun onDestroy() {
    mainSceneObject.stop()
  }
}
*/