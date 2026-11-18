package de.bauplan.live;

import net.fabricmc.api.ClientModInitializer;
import net.fabricmc.fabric.api.client.event.lifecycle.v1.ClientChunkEvents;
import net.fabricmc.fabric.api.client.event.lifecycle.v1.ClientLifecycleEvents;
import net.fabricmc.fabric.api.client.event.lifecycle.v1.ClientTickEvents;
import net.fabricmc.fabric.api.client.networking.v1.ClientPlayConnectionEvents;
import net.minecraft.client.Minecraft;
import net.minecraft.network.chat.Component;

import java.util.List;

/**
 * Bauplan im eigenen Spiel (z. B. Modrinth): Die Website läuft auf diesem PC unter localhost und zeigt,
 * was das Spiel gerade geladen hat. Auf dem Server muss nichts installiert sein.
 */
public class LiveClient implements ClientModInitializer {
	private static boolean told = false;

	@Override
	public void onInitializeClient() {
		Live.levels = () -> {
			Minecraft mc = Minecraft.getInstance();
			return mc.level == null ? List.of() : List.of(mc.level);
		};
		ClientLifecycleEvents.CLIENT_STARTED.register(mc -> Live.startWeb("localhost"));
		ClientLifecycleEvents.CLIENT_STOPPING.register(mc -> Live.stopWeb());
		ClientChunkEvents.CHUNK_LOAD.register((level, chunk) -> Changes.chunkLoaded(level, chunk.getPos().x(), chunk.getPos().z()));
		ClientTickEvents.END_CLIENT_TICK.register(mc -> {
			if (mc.level == null) return;
			Stand.radius = Math.max(4, Math.min(16, mc.options.getEffectiveRenderDistance())); // so weit, wie das Spiel geladen hat
			Live.tickCommon();
		});
		ClientPlayConnectionEvents.JOIN.register((handler, sender, mc) -> {
			var sd = mc.getCurrentServer();
			Live.switchWorld(sd != null ? sd.ip : "einzelspieler");
			if (!told) {
				told = true;
				mc.execute(() -> {
					if (mc.player != null) mc.player.sendSystemMessage(Component.literal("Bauplan Live ist bereit. Für das Bauplan-Programm: J → Einstellungen → Live-Link kopieren."));
				});
			}
		});
	}
}
