# SceneFlow

Was soll ScenFlow machen?

Eine einfache M�glichkeit bieten, um den Ablauf von verschiedenen Szenen nacheinander und gleichzeitig zu steuern.
Basierend auf Addressables und ScriptableObjects.

In einer Init Scene werden ScriptableObjects verkn�pft die den Ablauf der Szenen steuern.

Es braucht einen Trigger der die n�chste Scene startet.


# SceneFlow-Node
Eine Node ist die kleineste Steuerungseinheit in SceneFlow.
Sie enth�lt die zuladenenden Scenen, den Trigger um gestartet zu werden und 