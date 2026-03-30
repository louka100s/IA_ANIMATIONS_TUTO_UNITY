using UnityEngine;

/// <summary>
/// Configure un ParticleSystem au runtime selon le type de case du plateau.
/// A attacher sur chaque prefab de particules. Le champ tileType est defini
/// par BoardManager au moment de l'instanciation via Configure().
/// </summary>
[RequireComponent(typeof(ParticleSystem))]
public class TileParticleConfigurator : MonoBehaviour
{
    public enum TileParticleType
    {
        Gold,
        Heal,
        Danger,
        Artifact,
        Clue,
        Minigame
    }

    /// <summary>Type de case, definit l'apparence des particules.</summary>
    public TileParticleType tileType = TileParticleType.Gold;

    private ParticleSystem ps;

    private void Awake()
    {
        ps = GetComponent<ParticleSystem>();
        ApplyConfiguration();
    }

    /// <summary>Applique les parametres du ParticleSystem selon le tileType.</summary>
    private void ApplyConfiguration()
    {
        var main     = ps.main;
        var emission = ps.emission;
        var shape    = ps.shape;
        var renderer = ps.GetComponent<ParticleSystemRenderer>();

        // Valeurs communes
        main.loop            = true;
        main.playOnAwake     = true;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.gravityModifier = new ParticleSystem.MinMaxCurve(0f);
        emission.enabled     = true;
        shape.enabled        = true;

        switch (tileType)
        {
            // Pluie doree qui retombe : cone incline vers le bas, peu de particules
            case TileParticleType.Gold:
                main.startColor    = new ParticleSystem.MinMaxGradient(
                    new Color(1f, 0.843f, 0f, 1f),   // #FFD700
                    new Color(1f, 0.6f,  0f, 0.6f)); // orange translucide
                main.startSize     = new ParticleSystem.MinMaxCurve(0.05f, 0.1f);
                main.startLifetime = new ParticleSystem.MinMaxCurve(1.2f, 1.8f);
                main.startSpeed    = new ParticleSystem.MinMaxCurve(0.2f, 0.4f);
                main.gravityModifier = new ParticleSystem.MinMaxCurve(0.05f); // legere chute
                emission.rateOverTime = new ParticleSystem.MinMaxCurve(3f);
                shape.shapeType   = ParticleSystemShapeType.Cone;
                shape.angle       = 25f;
                shape.radius      = 0.25f;
                renderer.renderMode = ParticleSystemRenderMode.Billboard;
                break;

            // Bulles vertes qui montent lentement, grossissent et disparaissent
            case TileParticleType.Heal:
                main.startColor    = new ParticleSystem.MinMaxGradient(
                    new Color(0.3f, 1f, 0.5f, 0.9f),  // vert vif
                    new Color(0.5f, 1f, 0.7f, 0.4f)); // vert pale translucide
                main.startSize     = new ParticleSystem.MinMaxCurve(0.08f, 0.18f);
                main.startLifetime = new ParticleSystem.MinMaxCurve(2f, 3f);
                main.startSpeed    = new ParticleSystem.MinMaxCurve(0.08f, 0.18f);
                emission.rateOverTime = new ParticleSystem.MinMaxCurve(2f);
                shape.shapeType   = ParticleSystemShapeType.Sphere;
                shape.radius      = 0.35f;
                renderer.renderMode = ParticleSystemRenderMode.Billboard;
                ApplySizeOverLifetime(ps); // gonfle puis disparait
                break;

            // Brume violette stagnante tres basse vitesse, grosses particules
            case TileParticleType.Danger:
                main.startColor    = new ParticleSystem.MinMaxGradient(
                    new Color(0.6f, 0.1f, 0.8f, 0.35f),  // #9922CC semi-transparent
                    new Color(0.3f, 0f,   0.5f, 0.15f)); // violet profond discret
                main.startSize     = new ParticleSystem.MinMaxCurve(0.25f, 0.4f);
                main.startLifetime = new ParticleSystem.MinMaxCurve(2f, 3.5f);
                main.startSpeed    = new ParticleSystem.MinMaxCurve(0f, 0.05f); // quasi immobile
                emission.rateOverTime = new ParticleSystem.MinMaxCurve(2f);
                shape.shapeType   = ParticleSystemShapeType.Box;
                shape.scale       = new Vector3(0.8f, 0.05f, 0.8f); // nappe horizontale rase
                renderer.renderMode = ParticleSystemRenderMode.Billboard;
                ApplySizeOverLifetime(ps);
                break;

            // Etincelles orangees en anneau tres rapides, courte duree de vie
            case TileParticleType.Artifact:
                main.startColor    = new ParticleSystem.MinMaxGradient(
                    new Color(1f, 0.8f, 0.1f, 1f),   // #FFCC1A
                    new Color(1f, 0.4f, 0f,   0.8f)); // orange brule
                main.startSize     = new ParticleSystem.MinMaxCurve(0.04f, 0.07f);
                main.startLifetime = new ParticleSystem.MinMaxCurve(0.5f, 0.9f);
                main.startSpeed    = new ParticleSystem.MinMaxCurve(0.3f, 0.6f);
                emission.rateOverTime = new ParticleSystem.MinMaxCurve(4f);
                shape.shapeType   = ParticleSystemShapeType.Circle;
                shape.radius      = 0.35f;
                shape.radiusThickness = 0f; // bord uniquement
                renderer.renderMode = ParticleSystemRenderMode.Stretch;
                renderer.lengthScale = 3f;
                break;

            // Lucioles cyan qui derivent lentement sur un segment (effet orbite)
            case TileParticleType.Clue:
                main.startColor    = new ParticleSystem.MinMaxGradient(
                    new Color(0.1f, 0.9f, 0.8f, 1f),   // #1AE6CC
                    new Color(0.4f, 1f,   0.9f, 0.5f)); // cyan clair discret
                main.startSize     = new ParticleSystem.MinMaxCurve(0.06f, 0.1f);
                main.startLifetime = new ParticleSystem.MinMaxCurve(2f, 3f);
                main.startSpeed    = new ParticleSystem.MinMaxCurve(0.05f, 0.15f);
                emission.rateOverTime = new ParticleSystem.MinMaxCurve(2f);
                shape.shapeType   = ParticleSystemShapeType.Circle;
                shape.radius      = 0.4f;
                shape.radiusThickness = 0.2f;
                renderer.renderMode = ParticleSystemRenderMode.Billboard;
                ApplyColorOverLifetime(ps, new Color(0.1f, 0.9f, 0.8f, 1f), new Color(0.1f, 0.9f, 0.8f, 0f));
                break;

            // Eclats rouges vers le haut en cone serre, burst periodique
            case TileParticleType.Minigame:
                main.startColor    = new ParticleSystem.MinMaxGradient(
                    new Color(1f, 0.15f, 0.15f, 1f),   // #FF2626
                    new Color(1f, 0.5f,  0f,    0.8f)); // orange-rouge
                main.startSize     = new ParticleSystem.MinMaxCurve(0.05f, 0.09f);
                main.startLifetime = new ParticleSystem.MinMaxCurve(0.8f, 1.2f);
                main.startSpeed    = new ParticleSystem.MinMaxCurve(0.3f, 0.55f);
                emission.rateOverTime = new ParticleSystem.MinMaxCurve(0f); // uniquement bursts
                emission.SetBursts(new[] { new ParticleSystem.Burst(0f, 4, 6, 999, 1.5f) });
                shape.shapeType   = ParticleSystemShapeType.Cone;
                shape.angle       = 15f;
                shape.radius      = 0.15f;
                renderer.renderMode = ParticleSystemRenderMode.Billboard;
                break;
        }

        ps.Play();
    }

    /// <summary>Courbe de taille : gonfle puis disparait (Heal et Danger).</summary>
    private static void ApplySizeOverLifetime(ParticleSystem target)
    {
        var sol = target.sizeOverLifetime;
        sol.enabled = true;
        sol.size = new ParticleSystem.MinMaxCurve(1f, new AnimationCurve(
            new Keyframe(0f,   0.2f),
            new Keyframe(0.4f, 1f),
            new Keyframe(1f,   0f)
        ));
    }

    /// <summary>Fondu alpha de startColor vers endColor sur la duree de vie (Clue).</summary>
    private static void ApplyColorOverLifetime(ParticleSystem target, Color startColor, Color endColor)
    {
        var col = target.colorOverLifetime;
        col.enabled = true;
        var gradient = new Gradient();
        gradient.SetKeys(
            new[] { new GradientColorKey(startColor, 0f), new GradientColorKey(endColor, 1f) },
            new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0f, 1f) }
        );
        col.color = new ParticleSystem.MinMaxGradient(gradient);
    }
}
