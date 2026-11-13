"use client";

import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs";
import { Skeleton } from "@/components/ui/skeleton";
import { PageHeader } from "@/components/shared/page-header";
import { ErrorState } from "@/components/shared/error-state";
import { useProfile } from "@/hooks/use-api";
import { AvatarCard } from "./avatar-card";
import { ChangePasswordForm } from "./change-password-form";
import { ProfileForm } from "./profile-form";
import { ReminderSettings } from "./reminder-settings";

export function ProfilePage() {
  const { data, error, isLoading, refetch } = useProfile();
  return (
    <div>
      <PageHeader title="Hồ sơ cá nhân" description="Thông tin thể trạng giúp hệ thống tính mục tiêu calo phù hợp với bạn." />
      {isLoading ? (
        <div className="space-y-4">
          <Skeleton className="h-32 rounded-2xl" />
          <Skeleton className="h-64 rounded-2xl" />
        </div>
      ) : error || !data ? (
        <ErrorState error={error} onRetry={() => void refetch()} />
      ) : (
        <Tabs defaultValue="info" className="space-y-5">
          <TabsList>
            <TabsTrigger value="info">Thông tin</TabsTrigger>
            <TabsTrigger value="reminders">Nhắc nhở</TabsTrigger>
            <TabsTrigger value="security">Bảo mật</TabsTrigger>
          </TabsList>
          <TabsContent value="info" className="space-y-5">
            <AvatarCard profile={data} />
            <ProfileForm profile={data} />
          </TabsContent>
          <TabsContent value="reminders">
            <ReminderSettings />
          </TabsContent>
          <TabsContent value="security">
            <ChangePasswordForm />
          </TabsContent>
        </Tabs>
      )}
    </div>
  );
}
