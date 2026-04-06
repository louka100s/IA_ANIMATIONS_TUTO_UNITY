Concept
Le joueur incarne José, un archéologue qui explore un temple ancien au cœur de la jungle africaine. Son objectif : retrouver sa fille Lucia, trafiquante d'antiquités disparue dans les profondeurs du temple. Le jeu mélange un plateau de type Loop Hero avec un mini-jeu de cache-cache intégré.
Le plateau principal
Le plateau est composé de 20 cases disposées en boucle rectangulaire. Le joueur lance un dé et avance son pion (un cube gris) case par case. Chaque case a un effet particulier :

Cases normales : rien ne se passe, le joueur continue.
Cases or : José trouve des pièces anciennes (+20 or).
Cases danger : un piège se déclenche, dard empoisonné ou sol qui s'effondre (-15 PV).
Cases soin : une source d'eau claire permet de récupérer (+25 PV).
Cases dialogue : un texte narratif apparaît. Les dialogues évoluent en fonction du nombre de boucles parcourues. Au fil des tours, le temple devient plus menaçant et les indices se précisent.
Cases artéfact (x3) : chaque artéfact révèle un secret du temple. L'Amulette du Crépuscule, la Lame Oubliée du roi Kwame et l'Œil de Pierre sont les trois reliques nécessaires pour comprendre l'architecture du lieu.
Cases indice (x3) : chaque indice rapproche José de Lucia. Un carnet, des empreintes de pas, un message griffonné sur un mur.
Case mini-jeu : déclenche le cache-cache (voir ci-dessous).

Quand le joueur complète une boucle entière (retour à la case 0), il gagne un bonus de 10 or et la sauvegarde automatique se déclenche.
Le mini-jeu : cache-cache
Quand le pion atterrit sur la case mini-jeu (rouge), un panneau s'affiche avec un bouton "Entrer". Le joueur est alors transporté dans une scène dédiée.
Dans cette scène, le joueur contrôle un cube et doit atteindre une zone verte (objectif) sans se faire attraper par Erika Archer, une IA qui patrouille dans un petit labyrinthe. L'IA fonctionne avec une machine à états (Patrol, Follow, Attack) et utilise un système de détection par dot product combiné à un raycast.
Le labyrinthe contient des murs et des caisses en bois. Le joueur peut se cacher dans une caisse avec un clic droit quand il est à proximité. La cachette dure 5 secondes maximum avec un décompte visible à l'écran. Si l'IA voyait le joueur au moment où il entre dans la caisse, elle se dirige vers la caisse, la casse et le joueur est éjecté. Si l'IA ne l'avait pas repéré, elle continue sa patrouille sans rien remarquer.
Un timer de 60 secondes limite la durée du mini-jeu.
Conditions de victoire/défaite du mini-jeu :

Victoire : atteindre la zone verte → +50 or sur le plateau principal
Défaite : l'IA attrape le joueur ou le timer tombe à 0 → -25 or

Un écran de résultat s'affiche avant le retour au plateau avec un bouton "Continuer".
Lien entre le mini-jeu et le jeu principal
Le GameManager est un singleton persistant (DontDestroyOnLoad) qui stocke toutes les données du joueur : or, points de vie, nombre de boucles, artéfacts trouvés, indices collectés. Quand le mini-jeu se termine, le résultat est appliqué directement aux ressources du joueur avant le retour au plateau. Le joueur reprend sa position exacte sur la case où il se trouvait avant d'entrer dans le mini-jeu.
Conditions de fin du jeu

Victoire : réunir les 3 artéfacts et les 3 indices. José retrouve Lucia dans la chambre royale du temple.
Défaite : tomber à 0 PV ou 0 or. Le temple se referme sur José.

Fonctionnalités supplémentaires

Sauvegarde automatique à chaque boucle complétée (fichier JSON dans le dossier persistant)
HUD avec or, PV, boucles, artéfacts et indices collectés
Notification visuelle de sauvegarde
Écran titre avec possibilité de continuer ou recommencer une nouvelle partie
Difficulté progressive du mini-jeu : l'IA accélère et le timer diminue à chaque tentative


Structure des scènes

TitleScene : écran titre
SampleScene : plateau principal avec le circuit de cases
HideAndSeekScene : mini-jeu cache-cache avec l'IA Archer

Contrôles

Plateau : clic ou bouton pour lancer le dé
Mini-jeu : ZQSD pour se déplacer, clic droit pour se cacher dans une caisse
