using fbognini.Notifications.Services;

namespace fbognini.Notifications.Sinks.MTarget
{
    public static class Startup
    {
        public static MTargetSinkBuilder AddMTargetService(this SinkBuilder builder)
        {
            return new MTargetSinkBuilder(builder.Services)
                .AddMTargetService();
        }

        public static MTargetSinkBuilder AddMTargetService(this SinkBuilder builder, string id)
        {
            return new MTargetSinkBuilder(builder.Services)
                .AddMTargetService(id);
        }
    }
}
