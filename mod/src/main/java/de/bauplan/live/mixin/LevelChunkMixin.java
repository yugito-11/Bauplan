package de.bauplan.live.mixin;

import de.bauplan.live.Changes;
import net.minecraft.core.BlockPos;
import net.minecraft.world.level.block.state.BlockState;
import net.minecraft.world.level.chunk.LevelChunk;
import org.spongepowered.asm.mixin.Mixin;
import org.spongepowered.asm.mixin.injection.At;
import org.spongepowered.asm.mixin.injection.Inject;
import org.spongepowered.asm.mixin.injection.callback.CallbackInfoReturnable;

/**
 * Meldet jede echte Blockänderung (Spieler, Kolben, Explosion, Befehl …) an Bauplan.
 * Kostet fast nichts: ohne offene Live-Verbindung kehrt Changes.record sofort zurück.
 */
@Mixin(LevelChunk.class)
public abstract class LevelChunkMixin {
	@Inject(method = "setBlockState", at = @At("RETURN"))
	private void bauplan$changed(BlockPos pos, BlockState state, int flags, CallbackInfoReturnable<BlockState> cir) {
		if (cir.getReturnValue() == null) return; // nichts geändert
		Changes.record(((LevelChunk) (Object) this).getLevel(), pos, state);
	}
}
