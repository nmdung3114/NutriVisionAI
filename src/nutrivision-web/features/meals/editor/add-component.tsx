"use client";

import { useEffect, useId, useRef, useState } from "react";
import { useQuery } from "@tanstack/react-query";
import { Loader2, Plus, Search } from "lucide-react";
import { Input } from "@/components/ui/input";
import { Button } from "@/components/ui/button";
import { foodsApi } from "@/lib/api/foods";
import { useDebounced } from "@/hooks/use-debounced";
import { fmt } from "@/lib/meal";
import type { Food } from "@/types/api";

/** Ô tìm thực phẩm có gợi ý (autocomplete) – debounce 300ms gọi /api/foods?q= */
export function AddComponent({ onPickFood, onAddCustom }: { onPickFood: (f: Food) => void; onAddCustom: (name: string) => void }) {
  const [text, setText] = useState("");
  const [open, setOpen] = useState(false);
  const [active, setActive] = useState(-1);
  const q = useDebounced(text.trim(), 300);
  const listId = useId();
  const box = useRef<HTMLDivElement>(null);

  const { data, isFetching } = useQuery({
    queryKey: ["foods", "search", q],
    queryFn: ({ signal }) => foodsApi.search(q, 8, undefined, signal),
    enabled: q.length >= 1,
    staleTime: 60_000,
  });
  const results = q ? (data ?? []) : [];

  useEffect(() => {
    const onDoc = (e: MouseEvent) => {
      if (box.current && !box.current.contains(e.target as Node)) setOpen(false);
    };
    document.addEventListener("mousedown", onDoc);
    return () => document.removeEventListener("mousedown", onDoc);
  }, []);

  const pick = (f: Food) => {
    onPickFood(f);
    setText("");
    setOpen(false);
    setActive(-1);
  };

  return (
    <div ref={box} className="relative">
      <div className="relative">
        <Search className="pointer-events-none absolute left-3 top-1/2 size-4 -translate-y-1/2 text-muted-foreground" />
        <Input
          value={text}
          onChange={(e) => {
            setText(e.target.value);
            setOpen(true);
            setActive(-1);
          }}
          onFocus={() => setOpen(true)}
          onKeyDown={(e) => {
            if (e.key === "ArrowDown") {
              e.preventDefault();
              setActive((a) => Math.min(a + 1, results.length - 1));
            } else if (e.key === "ArrowUp") {
              e.preventDefault();
              setActive((a) => Math.max(a - 1, 0));
            } else if (e.key === "Enter") {
              e.preventDefault();
              if (active >= 0 && results[active]) pick(results[active]);
              else if (results.length === 1) pick(results[0]);
            } else if (e.key === "Escape") setOpen(false);
          }}
          role="combobox"
          aria-expanded={open}
          aria-controls={listId}
          aria-autocomplete="list"
          placeholder="+ Thêm thành phần: gõ để tìm thực phẩm (vd: cơm, trứng, rau muống…)"
          className="h-11 rounded-xl bg-card pl-9"
          aria-label="Tìm và thêm thành phần"
        />
        {isFetching && <Loader2 className="absolute right-3 top-1/2 size-4 -translate-y-1/2 animate-spin text-muted-foreground" />}
      </div>
      {open && text.trim() && (
        <ul id={listId} role="listbox" className="absolute z-30 mt-1.5 max-h-72 w-full overflow-y-auto rounded-xl border bg-popover p-1 shadow-lg">
          {results.map((f, i) => (
            <li key={f.id} role="option" aria-selected={i === active}>
              <button
                type="button"
                onMouseDown={(e) => e.preventDefault()}
                onClick={() => pick(f)}
                className={`flex w-full items-center justify-between gap-3 rounded-lg px-3 py-2 text-left text-sm hover:bg-accent ${i === active ? "bg-accent" : ""}`}
              >
                <span className="min-w-0">
                  <span className="block truncate font-semibold">{f.tenThucPham}</span>
                  <span className="block truncate text-xs text-muted-foreground">{f.nhomThucPham}</span>
                </span>
                <span className="shrink-0 text-xs font-semibold text-primary">{fmt(f.caloTren100g)} kcal/100g</span>
              </button>
            </li>
          ))}
          {!isFetching && results.length === 0 && q === text.trim() && <li className="px-3 py-2 text-sm text-muted-foreground">Không tìm thấy thực phẩm phù hợp.</li>}
          <li className="border-t p-1">
            <Button
              type="button"
              variant="ghost"
              size="sm"
              className="w-full justify-start text-primary"
              onMouseDown={(e) => e.preventDefault()}
              onClick={() => {
                onAddCustom(text.trim());
                setText("");
                setOpen(false);
              }}
            >
              <Plus /> Thêm &quot;{text.trim()}&quot; và tự nhập dinh dưỡng
            </Button>
          </li>
        </ul>
      )}
    </div>
  );
}
