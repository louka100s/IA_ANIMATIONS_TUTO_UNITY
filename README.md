Le Temple de Kwame — Jeu Unity

Concept

On joue José, un archéologue qui explore un temple ancien dans la jungle africaine pour retrouver sa fille Lucia, trafiquante d'antiquités disparue dans le temple. Le jeu est un plateau type Loop Hero avec deux mini-jeux qui font gagner ou perdre des ressources au joueur.
Le plateau
Le plateau fait 20 cases en boucle rectangulaire. On lance un dé, le pion (un cube gris) avance du nombre de cases correspondant. Selon la case sur laquelle on tombe, il se passe différents trucs :

Cases normales : rien
Cases or (jaunes) : +20 or
Cases danger (violettes) : -15 PV avec un piège qui se déclenche
Cases soin (vert clair) : +25 PV
Cases dialogue (bleues) : un texte apparaît, et le contenu change selon le nombre de boucles déjà faites (au début c'est calme, après ça devient inquiétant)
Cases artéfact (orange doré, x3) : on récupère un des trois artéfacts du roi Kwame (l'Amulette du Crépuscule, la Lame Oubliée, l'Œil de Pierre)
Cases indice (cyan, x3) : on trouve un indice sur Lucia (carnet, empreintes, message sur un mur)
Case mini-jeu cache-cache (rouge) : déclenche le cache-cache
Case mini-jeu RPS (orange) : déclenche le pierre feuille ciseaux

Quand on complète une boucle entière (retour à la case 0), on gagne 10 or de bonus et la sauvegarde se fait automatiquement.
Mini-jeu n°1 : cache-cache
Quand on tombe sur la case rouge, un panneau s'affiche avec un bouton "Entrer". Si on clique, on est transporté dans une scène séparée.
Là on contrôle un cube blanc et il faut atteindre une zone verte sans se faire choper par Erika Archer, l'IA qui patrouille dans un labyrinthe. L'IA a une machine à états (Patrol, Follow, Break Crate) et utilise un dot product avec un raycast pour vérifier qu'elle voit vraiment le joueur (pas à travers un mur). Quand elle perd le joueur de vue pendant 3 secondes, elle revient au point de patrouille le plus proche et reprend sa ronde.
Le labyrinthe a des murs et des caisses en bois. On peut se cacher dans une caisse avec clic droit quand on est à proximité. La cachette dure 5 secondes max avec un décompte à l'écran.
Le truc important : si l'IA voyait le joueur quand il rentre dans la caisse (état Follow), elle vient direct vers la caisse, la fait trembler et la casse. Le joueur est éjecté devant elle, vulnérable. Si elle ne le voyait pas (état Patrol), elle continue tranquille sans rien remarquer. Un texte "REPÉRÉ" en rouge ou "Caché" en vert indique au joueur sa situation en temps réel.
Il y a un timer de 60 secondes. La difficulté monte progressivement : à chaque tentative, l'IA va plus vite et le timer se réduit de 5 secondes.
Conditions :

Victoire : atteindre la zone verte → +50 or
Défaite : se faire attraper ou timer à 0 → -25 or

Un écran de résultat s'affiche avant de revenir au plateau.
Mini-jeu n°2 : pierre feuille ciseaux
Quand on tombe sur la case orange, on est transporté dans une scène RPS. C'est plus rapide et léger que le cache-cache, ça permet de varier.
L'interface affiche trois boutons (Pierre, Feuille, Ciseaux), un texte de résultat et un score. On clique notre choix, l'IA tire au hasard, on compare avec la formule classique ((playerChoice + 1) % 3 == aiChoice détermine si on gagne). En cas d'égalité on rejoue.
La partie se joue en 3 manches gagnantes. Quand quelqu'un atteint 3, les boutons sont désactivés et un bouton "Continuer" apparaît pour revenir au plateau.
Conditions :

Victoire : 3 manches gagnées → +50 or
Défaite : l'IA gagne 3 manches → -25 or

Lien entre les mini-jeux et le jeu principal
Le GameManager est un singleton avec DontDestroyOnLoad, donc il est conservé entre les scènes. Il stocke toutes les données du joueur : or, PV, boucles, artéfacts, indices, position sur le plateau. Quand un mini-jeu se termine, EndMiniGame() applique le résultat puis charge la scène du plateau. Le pion est replacé exactement à la case où on était avant.
Sans cette persistance, l'or perdu ou gagné dans les mini-jeux disparaîtrait au retour, donc tout le système est basé là-dessus.
Conditions de fin du jeu principal

Victoire : récupérer les 3 artéfacts ET les 3 indices. José retrouve Lucia dans la chambre royale, écran narratif final.
Défaite : tomber à 0 PV ou 0 or. Le temple se referme sur José avec un récap des progrès.

Sauvegarde
La sauvegarde se fait toute seule à chaque boucle complétée. Les données sont écrites en JSON via JsonUtility dans Application.persistentDataPath/save.json. Au lancement, le GameManager appelle SaveSystem.Load() qui restaure tout (or, PV, boucles, artéfacts, indices, position, tentatives du cache-cache).
Le HUD montre toujours l'état de la sauvegarde ("Boucle X sauvegardée" ou "Pas de sauvegarde") et une notif verte flash à chaque sauvegarde auto. Sur l'écran titre, soit on continue la partie sauvegardée, soit on clique "Nouvelle partie" pour effacer le fichier et tout recommencer.
Autres trucs ajoutés

HUD complet avec or, PV, boucles, compteurs artéfacts (3 max) et indices (3 max)
Textes flottants au-dessus du joueur quand il gagne ou perd quelque chose ("+20 or", "-15 PV")
Particules sur les cases spéciales pour bien les distinguer
Petit effet de bounce sur le cube à chaque atterrissage
Animation du dé qui défile des chiffres avant d'afficher le vrai résultat
Fondu au noir entre les transitions de scène
Écran titre avec texte qui clignote
Difficulté progressive du cache-cache

Scènes

TitleScene : écran titre avec animation et boutons Continuer / Nouvelle partie
SampleScene : plateau principal avec les 20 cases
HideAndSeekScene : mini-jeu cache-cache
RPSScene : mini-jeu pierre feuille ciseaux

Scripts

GameManager.cs : singleton persistant, stocke les données et gère les changements de scène
BoardManager.cs : génère le plateau de 20 cases avec leurs types et couleurs
PlayerController.cs : déplace le pion case par case et déclenche les effets selon le type de case
DiceRoller.cs : bouton de lancer de dé et HUD principal
SaveSystem.cs : sérialisation JSON
SaveHUD.cs : affichage de l'état de sauvegarde
IA_ARCHER_CONTROLLER.cs : machine à états de l'IA cache-cache
IA_Detection.cs : détection par dot product + raycast
MiniGamePlayer.cs : contrôle du cube dans le cache-cache, gestion des caisses
CatchPlayer.cs : détection collision IA-joueur (défaite)
GoalZone.cs : zone verte de victoire
MiniGameHUD.cs : timer, statut, indicateur de cachette
ResultScreen.cs : écran de résultat fin de mini-jeu
RPSGame.cs : logique du pierre feuille ciseaux

Contrôles

Plateau : bouton "Lancer le dé" ou touche Espace
Cache-cache : ZQSD pour bouger, clic droit pour se cacher dans une caisse
RPS : clic sur les boutons

Notes techniques
Pour le cache-cache, j'ai utilisé le NavMesh d'Unity pour le pathfinding de l'IA. Le sol et les obstacles statiques doivent être marqués Navigation Static avant le bake du NavMesh, et il faut penser à le rebake quand on déplace des trucs sinon l'IA reste bloquée (j'ai galéré là-dessus pendant un moment, je rebakeais pas après avoir bougé les murs).
Au début je voulais faire bouger le pion avec un Rigidbody et des forces physiques, mais comme on voulait un déplacement case par case discret j'ai fini par tout passer en Vector3.MoveTowards dans une coroutine, c'est beaucoup plus propre pour ce genre de mouvement.
L'IA utilise un NavMeshAgent pour bouger et un dot product + raycast pour confirmer qu'elle voit réellement le joueur (pas à travers un mur).
Le système de cachette repose sur des tags : le joueur a le tag "Player", les caisses "HidingCrate". Les colliders en trigger gèrent la détection de proximité (pour proposer la cachette) et les collisions de fin de partie.
