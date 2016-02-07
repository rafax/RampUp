using System;
using System.Collections.Generic;
using RampUp.Actors.Impl;
using RampUp.Ring;

namespace RampUp.Actors.PluginModel
{
    /// <summary>
    /// Base class for features based on actors. Every feature provides two elements:
    /// - actors which are needed to coexist with an actor using this feature. They are obtained with <see cref="GetCoexistingActors"/>
    /// - actors which are needed to exist in the system (they are singletons, instansiated once)
    /// </summary>
    public interface IFeature
    {
        /// <summary>
        /// Gets actors coexisting with <paramref name="actor"/> to enable this feature. The actors will be run as <see cref="CompositeActor"/> on the same <see cref="IRingBuffer"/>.
        /// </summary>
        /// <param name="actor">The actor</param>
        /// <returns></returns>
        IEnumerable<IActor> GetCoexistingActors(IActor actor);

        /// <summary>
        /// Gets actors required for this feature to work.
        /// </summary>
        /// <returns></returns>
        IEnumerable<IActor> GetFeatureActors();
    }

    public sealed class ActorSystem
    {
        private readonly Dictionary<Type, IFeature> _features = new Dictionary<Type, IFeature>();
        private readonly List<IActor> _featureActors = new List<IActor>();

        public ActorSystem Add<TActor>(TActor actor, Action<RegistrationContext<TActor>> register)
            where TActor : IActor
        {
            var ctx = new RegistrationContext<TActor>(actor, this);
            register(ctx);

            //TODO: add all the actors to the same ring buffer, combine if needed, provide descriptor
            return this;
        }

        private TFeature GetOrAdd<TFeature>()
            where TFeature : IFeature, new()
        {
            IFeature feature;
            if (_features.TryGetValue(typeof (TFeature), out feature) == false)
            {
                feature = new TFeature();
                _featureActors.AddRange(feature.GetFeatureActors());
            }

            return (TFeature) feature;
        }

        public class RegistrationContext<TActor>
            where TActor : IActor
        {
            public readonly TActor Actor;
            internal readonly List<IActor> Actors = new List<IActor>();
            private readonly ActorSystem _actorSystem;

            internal RegistrationContext(TActor actor, ActorSystem actorSystem)
            {
                Actor = actor;
                _actorSystem = actorSystem;
                Actors.Add(actor);
            }

            public RegistrationContext<TActor> Use<TFeature>(Action<TActor, TFeature> configure)
                where TFeature : IFeature, new()
            {
                var feature = _actorSystem.GetOrAdd<TFeature>();
                Actors.AddRange(feature.GetCoexistingActors(Actor));
                configure(Actor, feature);
                return this;
            }
        }
    }
}