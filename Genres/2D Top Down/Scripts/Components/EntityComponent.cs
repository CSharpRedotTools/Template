using Godot;
using RedotUtils;

namespace Template.TopDown2D;

[GlobalClass, Icon(Images.GearIcon)]
public partial class EntityComponent : Node2D
{
    [Export] private EntityConfig _config;
    [Export] public AnimatedSprite2D AnimatedSprite { get; private set; }
    
    protected Node2D _entity;
    private ShaderMaterial _shaderMaterial;

    public override void _Ready()
    {
        _entity = GetOwner<Node2D>();
        _shaderMaterial = AnimatedSprite.Material as ShaderMaterial;

        SetMaterial();
        CreateReflection();
    }

    public virtual void TakeDamage(Vector2 direction = default)
    {
        if (_shaderMaterial != null)
        {
            StrobeFlash(this, _shaderMaterial);
        }
    }

    private static void StrobeFlash(Node node, ShaderMaterial shaderMaterial)
    {
        float initialDuration = 0.04f; // Initial duration for the first flash
        float durationIncrement = 0.01f; // Increment for each subsequent flash
        float intensityDecrement = 0.3f; // Decrement for each subsequent flash

        float currentDuration = initialDuration;
        float currentIntensity = 1.0f;

        // Create a sequence of tweens to simulate the strobe light effect
        RTween tween = new RTween(node).SetAnimatingShaderMaterial(shaderMaterial);

        for (int i = 0; i < 4; i++) // Adjust the number of flashes as needed
        {
            // Flash on
            tween.AnimateShader("blend_intensity", currentIntensity, currentDuration).EaseIn();

            // Flash off
            tween.AnimateShader("blend_intensity", 0.0f, currentDuration).EaseOut();

            // Update the duration and intensity for the next flash
            currentDuration += durationIncrement;
            currentIntensity -= intensityDecrement;

            // Ensure the intensity doesn't go below 0.0
            if (currentIntensity < 0.0f)
            {
                currentIntensity = 0.0f;
            }
        }
    }

    private void SetMaterial()
    {
        AnimatedSprite.SelfModulate = _config.Color;

        /*AnimatedSprite.Material ??= new CanvasItemMaterial()
        {
            BlendMode = _config.BlendMode,
            LightMode = _config.LightMode
        };*/
    }

    private void CreateReflection()
    {
        PuddleReflectionUtils.CreateReflection(_entity, AnimatedSprite);
    }
}
